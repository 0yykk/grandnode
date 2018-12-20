﻿using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;

namespace Grand.Api.Services
{
    public interface IManufacturerApiService
    {
        ManufacturerDto GetById(string id);
        IMongoQueryable<ManufacturerDto> GetManufacturers();
        ManufacturerDto InsertManufacturer(ManufacturerDto model);
        ManufacturerDto UpdateManufacturer(ManufacturerDto model);
        void DeleteManufacturer(ManufacturerDto model);
    }
}
