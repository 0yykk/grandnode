﻿using Grand.Core.Domain.Knowledgebase;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grand.Services.Knowledgebase
{
    public interface IKnowledgebaseService
    {
        /// <summary>
        /// Gets knowledgebase category
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase category</returns>
        KnowledgebaseCategory GetKnowledgebaseCategory(string id);

        /// <summary>
        /// Gets knowledgebase categories
        /// </summary>
        /// <returns>List of knowledgebase categories</returns>
        List<KnowledgebaseCategory> GetKnowledgebaseCategories();

        /// <summary>
        /// Inserts knowledgebase category
        /// </summary>
        /// <param name="kc"></param>
        void InsertKnowledgebaseCategory(KnowledgebaseCategory kc);

        /// <summary>
        /// Updates knowledgebase category
        /// </summary>
        /// <param name="kc"></param>
        void UpdateKnowledgebaseCategory(KnowledgebaseCategory kc);

        /// <summary>
        /// Deletes knowledgebase category
        /// </summary>
        /// <param name="kc"></param>
        void DeleteKnowledgebaseCategory(KnowledgebaseCategory kc);

        /// <summary>
        /// Gets knowledgebase article
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase article</returns>
        KnowledgebaseArticle GetKnowledgebaseArticle(string id);

        /// <summary>
        /// Gets knowledgebase articles
        /// </summary>
        /// <returns>List of knowledgebase articles</returns>
        List<KnowledgebaseArticle> GetKnowledgebaseArticles();

        /// <summary>
        /// Inserts knowledgebase article
        /// </summary>
        /// <param name="ka"></param>
        void InsertKnowledgebaseArticle(KnowledgebaseArticle ka);

        /// <summary>
        /// Updates knowledgebase article
        /// </summary>
        /// <param name="ka"></param>
        void UpdateKnowledgebaseArticle(KnowledgebaseArticle ka);

        /// <summary>
        /// Deletes knowledgebase article
        /// </summary>
        /// <param name="ka"></param>
        void DeleteKnowledgebaseArticle(KnowledgebaseArticle ka);
    }
}
