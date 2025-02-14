using RC.DBA.Metamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Query.GraphApi
{
    public class GraphQuery
    {
        public GraphFilter Filter;

        public ISelectQuery<T> ApplyQuery<T>(ISelectQuery<T> query, IModelManager modelManager)
        {
            if (Filter != null)
                Filter.ApplyFilter(query, modelManager);

            //query.Select().SelectAll();

            return query;
        }

        
    }

    public class GraphFilter
    {
        public string Op; // operation
        public string Prop; // property or field
        public string Value;

        public GraphFilter[] Filters;

        public void ApplyFilter<T>(ISelectQuery<T> query, IModelManager modelManager)
        {
            if (Prop != null)
            {
                var filter = query.Filter();
                filter.And(GeneratePredicate<T>(filter, modelManager));
            }
            else if (Filters != null && Filters.Length > 0)
            {
                var filter = query.Filter();
                var predicates = new Predicate[Filters.Length];
                for (int i = 0; i < Filters.Length; i++)
                {
                    predicates[i] = Filters[i].GeneratePredicate<T>(filter, modelManager);
                }
                filter.And(predicates);
            }

            //query.Filter()
        }

        public Predicate GeneratePredicate<T>(Filter<T> filter, IModelManager modelManager)
        {
            var etityAttribute = modelManager.Entity<T>().GetAttribute(Prop);
            var tProp = etityAttribute.MemberType;
            tProp = Helper.GetNonNullableType(tProp);
            var tGuid = tProp.GUID;

            if (Value == null || tGuid == Helper.TypeGuid.StringGuid)
            {
                var prop = filter.Prop<string>(Prop);
                return prop.Operator(Value, Op);
            }
            else if (tGuid == Helper.TypeGuid.IntGuid)
            {
                var prop = filter.Prop<int>(Prop);
                var iVal = int.Parse(Value);
                return prop.Operator(iVal, Op);
            }
            else if (tGuid == Helper.TypeGuid.BoolGuid)
            {
                var prop = filter.Prop<bool>(Prop);
                var bVal = bool.Parse(Value);
                return prop.Operator(bVal, Op);
            }
            else if (tGuid == Helper.TypeGuid.DateTimeGuid)
            {
                var prop = filter.Prop<DateTime>(Prop);
                var dtVal = DateTime.Parse(Value);
                return prop.Operator(dtVal, Op);
            }

            return null;
        }

           
    }
}
