using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Products.Specifications;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Specifications;
using QRDine.Application.Features.Sales.Repositories;
using QRDine.Domain.Catalog;
using QRDine.Domain.Enums;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.Orders.Services
{
    public class OrderCreationService : IOrderCreationService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ITableRepository _tableRepository;

        public OrderCreationService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ITableRepository tableRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _tableRepository = tableRepository;
        }

        public async Task<Order> CreateOrAppendOrderAsync(OrderCreationDto model, CancellationToken cancellationToken)
        {
            var table = await _tableRepository.GetByIdAsync(model.TableId, cancellationToken);

            if (table == null || table.MerchantId != model.MerchantId)
                throw new NotFoundException("Bàn không tồn tại hoặc không hợp lệ.");

            var productIds = model.Items.Select(x => x.ProductId).Distinct().ToList();

            var productSpec = new GetProductsByIdsSpec(model.MerchantId, productIds);
            var products = await _productRepository.ListAsync(productSpec, cancellationToken);

            if (products.Count != productIds.Count)
                throw new NotFoundException("Một số món ăn không tồn tại hoặc đã ngừng bán.");

            var orderSpec = new GetActiveOrderBySessionSpec(model.MerchantId, model.TableId, model.SessionId);
            var existingOrder = await _orderRepository.SingleOrDefaultAsync(orderSpec, cancellationToken);

            if (existingOrder != null)
            {
                AddItems(existingOrder, model, products);
                await _orderRepository.UpdateAsync(existingOrder, cancellationToken);
                return existingOrder;
            }

            var order = new Order
            {
                MerchantId = model.MerchantId,
                TableId = table.Id,
                TableName = table.Name,
                SessionId = model.SessionId,
                OrderCode = OrderCodeGenerator.Generate(),
                Status = OrderStatus.Pending,
                CustomerName = model.CustomerName,
                CustomerPhone = model.CustomerPhone,
                Note = model.Note,
                TotalAmount = 0
            };

            AddItems(order, model, products);
            await _orderRepository.AddAsync(order, cancellationToken);

            if (!table.IsOccupied)
            {
                table.IsOccupied = true;
                table.CurrentSessionId = model.SessionId;
                await _tableRepository.UpdateAsync(table, cancellationToken);
            }

            return order;
        }

        private void AddItems(Order order, OrderCreationDto model, List<Product> products)
        {
            foreach (var itemModel in model.Items)
            {
                var product = products.First(p => p.Id == itemModel.ProductId);
                var amount = (product.Price + itemModel.ToppingSurcharge) * itemModel.Quantity;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    ToppingsSnapshot = itemModel.ToppingsSnapshot,
                    Quantity = itemModel.Quantity,
                    Amount = amount,
                    Note = itemModel.Note
                });

                order.TotalAmount += amount;
            }
        }
    }
}
