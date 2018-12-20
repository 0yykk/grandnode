﻿using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;

namespace Grand.Api.Services
{
    public interface ICategoryApiService
    {
        CategoryDto GetById(string id);
        IMongoQueryable<CategoryDto> GetCategories();
        CategoryDto InsertCategory(CategoryDto model);
        CategoryDto UpdateCategory(CategoryDto model);
        void DeleteCategory(CategoryDto model);
    }
}
