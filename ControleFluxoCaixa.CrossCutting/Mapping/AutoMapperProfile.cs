using AutoMapper;
using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Domain.Entities;

namespace ControleFluxoCaixa.CrossCutting.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Itens → Lancamento (entrada do serviço para entidade de domínio)
            CreateMap<Itens, Lancamento>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid())) // Gera novo Guid
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo))   // Já é enum, mapeia direto
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data))
                .ForMember(dest => dest.Valor, opt => opt.MapFrom(src => src.Valor))
                .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Descricao));

            // Lancamento → ItenLancando (caso de retorno para leitura)
            CreateMap<Lancamento, ItenLancando>()
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo));

        }
    }
}
