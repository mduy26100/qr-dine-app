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
                    throw new ConflictException("Bàn này đang có khách. Bắt buộc phải truyền SessionId để gọi thêm món. Vui lòng tải lại trang!");

                if (model.SessionId.Value != table.CurrentSessionId)
                    throw new ConflictException("SessionId không khớp với phiên ăn hiện tại của bàn. Vui lòng tải lại trang để tránh nhầm bill!");

                activeSessionId = table.CurrentSessionId.Value;
            }
            else
            {
                activeSessionId = Guid.NewGuid();
            }

            var productIds = model.Items.Select(x => x.ProductId).Distinct().ToList();
            var productSpec = new GetProductsByIdsSpec(model.MerchantId, productIds);
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
                        {
                            existingOrder.Note = model.Note;
                        }
                        else if (!existingOrder.Note.Contains(model.Note))
                        {
                            existingOrder.Note += $" | Lần gọi thêm: {model.Note}";
                        }
                    }

                    if (string.IsNullOrWhiteSpace(existingOrder.CustomerName) && !string.IsNullOrWhiteSpace(model.CustomerName))
                        existingOrder.CustomerName = model.CustomerName;

                    if (string.IsNullOrWhiteSpace(existingOrder.CustomerPhone) && !string.IsNullOrWhiteSpace(model.CustomerPhone))
                        existingOrder.CustomerPhone = model.CustomerPhone;

                    AddItems(existingOrder, model, products);
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

                    AddItems(order, model, products);
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
                throw new ConflictException("Bàn này vừa được mở Order bởi một khách hàng khác. Vui lòng tải lại trang để gọi chung hóa đơn!");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private void AddItems(Order order, OrderCreationDto model, List<ProductPriceDto> products)
        {
            foreach (var itemModel in model.Items)
            {
                var product = products.First(p => p.Id == itemModel.ProductId);
                var amountToAdd = (product.Price + itemModel.ToppingSurcharge) * itemModel.Quantity;

                var existingItem = order.OrderItems.FirstOrDefault(oi =>
                    oi.Status == OrderItemStatus.Pending &&
                    oi.ProductId == product.Id &&
                    oi.ToppingsSnapshot == itemModel.ToppingsSnapshot &&
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
                        ToppingsSnapshot = itemModel.ToppingsSnapshot,
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