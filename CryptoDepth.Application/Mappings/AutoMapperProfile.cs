using AutoMapper;
using CryptoDepth.Domain.Dto;
using CryptoDepth.Domain.Entities;

namespace CryptoDepth.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<TradingPairDepth, TradingPairDepthDto>();
            CreateMap<TradingPairDepthDto, TradingPairDepth>();
        }
    }
}