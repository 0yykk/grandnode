﻿using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Data;
using Grand.Services.Catalog;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;

namespace Grand.Api.Services
{
    public partial class SpecificationAttributeApiService : ISpecificationAttributeApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IMongoCollection<SpecificationAttributeDto> _specificationAttribute;

        public SpecificationAttributeApiService(IMongoDBContext mongoDBContext, ISpecificationAttributeService specificationAttributeService)
        {
            _mongoDBContext = mongoDBContext;
            _specificationAttributeService = specificationAttributeService;
            _specificationAttribute = _mongoDBContext.Database().GetCollection<SpecificationAttributeDto>(typeof(Core.Domain.Catalog.SpecificationAttribute).Name);
        }
        public virtual SpecificationAttributeDto GetById(string id)
        {
            return _specificationAttribute.AsQueryable().FirstOrDefault(x => x.Id == id);
        }
        public virtual IMongoQueryable<SpecificationAttributeDto> GetSpecificationAttributes()
        {
            return _specificationAttribute.AsQueryable();
        }
        public virtual SpecificationAttributeDto InsertSpecificationAttribute(SpecificationAttributeDto model)
        {
            var specificationAttribute = model.ToEntity();
            _specificationAttributeService.InsertSpecificationAttribute(specificationAttribute);
            return specificationAttribute.ToModel();
        }

        public virtual SpecificationAttributeDto UpdateSpecificationAttribute(SpecificationAttributeDto model)
        {
            var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(model.Id);
            foreach (var option in specificationAttribute.SpecificationAttributeOptions)
            {
                if (model.SpecificationAttributeOptions.FirstOrDefault(x => x.Id == option.Id) == null)
                {
                    _specificationAttributeService.DeleteSpecificationAttributeOption(option);
                }
            }
            specificationAttribute = model.ToEntity(specificationAttribute);
            _specificationAttributeService.UpdateSpecificationAttribute(specificationAttribute);
            return specificationAttribute.ToModel();
        }
        public virtual void DeleteSpecificationAttribute(SpecificationAttributeDto model)
        {
            var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(model.Id);
            if (specificationAttribute != null)
                _specificationAttributeService.DeleteSpecificationAttribute(specificationAttribute);

        }
    }
}
