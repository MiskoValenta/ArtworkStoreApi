using ArtworkStoreApi.DTOs;
using ArtworkStoreApi.Models;
using AutoMapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<RegisterRequestDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore())
            .ForMember(dest => dest.Reviews, opt => opt.Ignore());

        // Artwork mappings
        CreateMap<Artwork, ArtworkDto>()
            .ForMember(dest => dest.GenreName, opt => opt.MapFrom(src => src.Genre.Name));
        CreateMap<ArtworkCreateDto, Artwork>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Genre, opt => opt.Ignore())
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
            .ForMember(dest => dest.Reviews, opt => opt.Ignore());

        // Genre mappings
        CreateMap<Genre, GenreDto>()
            .ForMember(dest => dest.ArtworksCount, opt => opt.MapFrom(src => src.Artworks.Count));
        CreateMap<GenreCreateDto, Genre>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Artworks, opt => opt.Ignore());

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));
        CreateMap<OrderCreateDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrderDate, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // OrderItem mappings
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ArtworkTitle, opt => opt.MapFrom(src => src.Artwork.Title))
            .ForMember(dest => dest.ArtworkImageUrl, opt => opt.MapFrom(src => src.Artwork.ImageUrl));
        CreateMap<OrderItemCreateDto, OrderItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
            .ForMember(dest => dest.Order, opt => opt.Ignore())
            .ForMember(dest => dest.Artwork, opt => opt.Ignore());

        // Review mappings
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.ArtworkTitle, opt => opt.MapFrom(src => src.Artwork.Title));
        CreateMap<ReviewCreateDto, Review>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsApproved, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Artwork, opt => opt.Ignore());
    }
}