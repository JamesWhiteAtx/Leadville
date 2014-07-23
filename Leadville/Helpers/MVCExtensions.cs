using System;
using System.Collections.Generic;
using System.Linq;
//using System.Web;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace System.Web.Mvc
{
    public static class MVCExtensions
    {
        public static string GetDisplayName<TModel, TProperty>(this TModel model, Expression<Func<TModel, TProperty>> expression)
        {
            return ModelMetadata.FromLambdaExpression<TModel, TProperty>(expression, new ViewDataDictionary<TModel>(model)).DisplayName;
        }

        public static string FullPropertyName<T, TReturn>(this T obj, Expression<Func<T, TReturn>> expression) where T : class
        {
            return ExpressionHelper.GetExpressionText(expression);
        }
    }
}