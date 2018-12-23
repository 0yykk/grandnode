﻿using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;

namespace Grand.Api.Services
{
    public interface ISpecificationAttributeApiService
    {
        SpecificationAttributeDto GetById(string id);
        IMongoQueryable<SpecificationAttributeDto> GetSpecificationAttributes();
        SpecificationAttributeDto InsertSpecificationAttribute(SpecificationAttributeDto model);
        SpecificationAttributeDto UpdateSpecificationAttribute(SpecificationAttributeDto model);
        void DeleteSpecificationAttribute(SpecificationAttributeDto model);
    }
}
