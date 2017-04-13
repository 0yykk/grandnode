﻿using Grand.Core.Domain.Catalog;
using Grand.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Grand.Web.Services
{
    public partial interface IProductWebService
    {
        IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(
            IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false);

        IList<ProductSpecificationModel> PrepareProductSpecificationModel(
            Product product);

        ProductReviewOverviewModel PrepareProductReviewOverviewModel(
           Product product);
    }
}