using QRDine.Application.Common.Abstractions.Persistence;
using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Application.Features.Catalog.Products.Specifications;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Specifications;
using QRDine.Application.Features.Sales.Repositories;
using QRDine.Domain.Enums;
using QRDine.Domain.Sales;
using System.Text.Json;

namespace QRDine.Application.Features.Sales.Orders.Services
{
    public class OrderCreationService : IOrderCreationService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ITableRepository _tableRepository;
        private readonly IApplicationDbContext _dbContext;

        public OrderCreationService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ITableRepository tableRepository,
            IApplicationDbContext dbContext)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _tableRepository = tableRepository;
            _dbContext = dbContext;
        }

        public async Task<Order> CreateOrAppendOrderAsync(OrderCreationDto model, CancellationToken cancellationToken)
        {
            var table = await _tableRepository.GetByIdAsync(model.TableId, cancellationToken);
            if (table == null || table.MerchantId != model.MerchantId)
                throw new NotFoundException("Bàn không tồn tại hoặc không hợp lệ.");

            Guid activeSessionId;
            if (table.IsOccupied)
            {
                if (!model.SessionId.HasValue)
                    throw new ConflictException("Bàn này đang có khách. Bắt buộc phải truyền SessionId để gọi thêm món.");

                if (model.SessionId.Value != table.CurrentSessionId)
                    throw new ConflictException("SessionId không khớp với phiên ăn hiện tại. Vui lòng tải lại trang để tránh nhầm bill!");

                activeSessionId = table.CurrentSessionId.Value;
            }
            else
            {
                activeSessionId = Guid.NewGuid();
            }

            var productIds = model.Items.Select(x => x.ProductId).Distinct().ToList();
            var productSpec = new GetProductsWithToppingsByIdsSpec(model.MerchantId, productIds);
            var products = await _productRepository.ListAsync(productSpec, cancellationToken);

            if (products.Count != productIds.Count)
                throw new NotFoundException("Một số món ăn không tồn tại hoặc đã ngừng bán.");

            Order? existingOrder = null;
            if (table.IsOccupied)
            {
                var orderSpec = new GetActiveOrderBySessionSpec(model.MerchantId, model.TableId, activeSessionId);
                existingOrder = await _orderRepository.SingleOrDefaultAsync(orderSpec, cancellationToken);
            }

            await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

            try
            {
                if (!table.IsOccupied)
                {
                    table.IsOccupied = true;
                    table.CurrentSessionId = activeSessionId;
                    await _tableRepository.UpdateAsync(table, cancellationToken);
                }

                Order orderToReturn;

                if (existingOrder != null)
                {
                    if (!string.IsNullOrWhiteSpace(model.Note))
                    {
                        if (string.IsNullOrWhiteSpace(existingOrder.Note))
                            existingOrder.Note = model.Note;
                        else if (!existingOrder.Note.Contains(model.Note))
                            existingOrder.Note += $" | Lần gọi thêm: {model.Note}";
                    }

                    if (string.IsNullOrWhiteSpace(existingOrder.CustomerName) && !string.IsNullOrWhiteSpace(model.CustomerName))
                        existingOrder.CustomerName = model.CustomerName;

                    if (string.IsNullOrWhiteSpace(existingOrder.CustomerPhone) && !string.IsNullOrWhiteSpace(model.CustomerPhone))
                        existingOrder.CustomerPhone = model.CustomerPhone;

                    ValidateAndAddItems(existingOrder, model, products);

                    await _orderRepository.UpdateAsync(existingOrder, cancellationToken);
                    orderToReturn = existingOrder;
                }
                else
                {
                    var order = new Order
                    {
                        MerchantId = model.MerchantId,
                        TableId = table.Id,
                        TableName = table.Name,
                        SessionId = activeSessionId,
                        OrderCode = OrderCodeGenerator.Generate(),
                        Status = OrderStatus.Open,
                        CustomerName = model.CustomerName,
                        CustomerPhone = model.CustomerPhone,
                        Note = model.Note,
                        TotalAmount = 0
                    };

                    ValidateAndAddItems(order, model, products);

                    await _orderRepository.AddAsync(order, cancellationToken);
                    orderToReturn = order;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return orderToReturn;
            }
            catch (ConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new ConflictException("Bàn này vừa được mở Order bởi một khách hàng khác.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private void ValidateAndAddItems(Order order, OrderCreationDto model, List<ProductWithToppingsDto> products)
        {
            foreach (var itemModel in model.Items)
            {
                var product = products.First(p => p.Id == itemModel.ProductId);
                if (!product.IsAvailable)
                    throw new BadRequestException($"Món '{product.Name}' hiện đang tạm hết.");

                var selectedToppings = new List<ProductToppingDto>();
                decimal calculatedToppingSurcharge = 0;

                if (itemModel.SelectedToppingIds != null && itemModel.SelectedToppingIds.Any())
                {
                    var allowedToppings = product.ToppingGroups
                        .Where(tg => tg.IsActive)
                        .SelectMany(tg => tg.Toppings)
                        .Where(t => t.IsAvailable)
                        .ToList();

                    foreach (var tId in itemModel.SelectedToppingIds)
                    {
                        var topping = allowedToppings.FirstOrDefault(x => x.Id == tId);
                        if (topping == null)
                            throw new BadRequestException($"Lựa chọn đính kèm không hợp lệ cho món '{product.Name}'.");

                        selectedToppings.Add(topping);
                        calculatedToppingSurcharge += topping.Price;
                    }
                }

                foreach (var group in product.ToppingGroups)
                {
                    if (!group.IsActive) continue;

                    var selectedCountInGroup = selectedToppings.Count(t => t.ToppingGroupId == group.Id);

                    if (group.IsRequired && selectedCountInGroup < group.MinSelections)
                        throw new BadRequestException($"Vui lòng chọn ít nhất {group.MinSelections} lựa chọn cho nhóm '{group.Name}' của món '{product.Name}'.");

                    if (selectedCountInGroup > group.MaxSelections)
                        throw new BadRequestException($"Nhóm '{group.Name}' của món '{product.Name}' chỉ cho phép chọn tối đa {group.MaxSelections} lựa chọn.");
                }

                var snapshotStr = selectedToppings.Any()
                    ? JsonSerializer.Serialize(selectedToppings.Select(t => new { t.Id, t.Name, t.Price }))
                    : null;

                var amountToAdd = (product.Price + calculatedToppingSurcharge) * itemModel.Quantity;

                var existingItem = order.OrderItems.FirstOrDefault(oi =>
                    oi.Status == OrderItemStatus.Pending &&
                    oi.ProductId == product.Id &&
                    oi.ToppingsSnapshot == snapshotStr &&
                    oi.Note == itemModel.Note);

                if (existingItem != null)
                {
                    existingItem.Quantity += itemModel.Quantity;
                    existingItem.Amount += amountToAdd;
                }
                else
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        UnitPrice = product.Price,
                        ToppingsSnapshot = snapshotStr,
                        Quantity = itemModel.Quantity,
                        Amount = amountToAdd,
                        Status = OrderItemStatus.Pending,
                        Note = itemModel.Note
                    });
                }

                order.TotalAmount += amountToAdd;
            }
        }
    }
}