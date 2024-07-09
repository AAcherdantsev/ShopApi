using AutoMapper;
using ShopApi.Models.Enums;
using ShopApi.Models.Orders;
using ShopApi.PublicModels.Orders;

namespace ShopApi.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<BaseOrderDto, Order>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.New))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<OrderDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created));

        CreateMap<OrderItemDto, OrderItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrderId, opt => opt.Ignore());

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created));

        CreateMap<OrderItem, OrderItemDto>();
    }
}