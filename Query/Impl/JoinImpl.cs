using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using RC.DBA.Metamodel;

namespace RC.DBA.Query.Impl
{
    class JoinImpl<T> : IJoin<T>
    {
        protected IAliasExpression _alias;

        protected JoinType _JoinType;
        protected QueryContext _ctx;
        protected IPathExpression _path;

        protected ISelect<T> _Select;
        //protected ISelectExpression _Projection;
        protected List<IJoin> _Joins;
        protected Filter<T> _Filter;
        protected IOrderBy<T> _OrderBy;
        public IAliasExpression Alias => _alias;

        public IPathExpression Path => _path;
        public JoinType JoinType
        {
            get { return _JoinType; }
            set { _JoinType = value; }
        }

        public Predicate Predicate { get => _Filter; }

        public JoinImpl(QueryContext ctx, IAliasExpression alias, IPathExpression path, List<IJoin> joins)
        {
            _alias = alias;
            _ctx = ctx;
            _path = path;
            _Joins = joins;
        }

        public virtual StringBuilder CompileToSQL(StringBuilder sql)
        {
            var entity = _ctx.ModelManager.Entity<T>();

            CompileJoin(sql, entity.Table);

            _Filter.CompileToSQL(sql);

            AddJoinForSelectedTables(sql, entity);

            return sql;
        }

        protected void AddJoinForSelectedTables(StringBuilder sql, IEntityType entity)
        {
            var select = _Select; //_Projection ?? _Select;
            if (select == null) return;
            
            var table = entity.Table;
            var parentTable = entity.ParentTable;
            var idCol = entity.Key?.Column;
            var tables = new HashSet<EntityTable>(select.Tables());
            AddFilterTables(tables);
            
            while (parentTable != null)
            {
                if (!tables.Contains(parentTable))
                {
                    parentTable = parentTable.Parent;
                    continue;
                }

                if (idCol == null)
                    throw new QueryException("Entity " + entity.ClassType.FullName + " must have primary Key");

                CompileJoin(sql, parentTable);

                table.CompileAlias(sql, _alias).Append('.').Append(idCol);
                sql.Append('=');
                parentTable.CompileAlias(sql, _alias).Append('.').Append(idCol);

                tables.Remove(parentTable);

                table = parentTable;
                parentTable = table.Parent;
            }

            table = entity.Table;
            foreach (var t in tables.Where(t => t != table))
            {
                if (!IsParent(table, t)) continue;
                if (idCol == null)
                    throw new QueryException("Entity " + entity.ClassType.FullName + " must have primary Key");

                CompileJoin(sql, t, true);

                table.CompileAlias(sql, _alias).Append('.').Append(idCol);
                sql.Append('=');
                t.CompileAlias(sql, _alias).Append('.').Append(idCol);
            }
        }

        private void AddFilterTables(HashSet<EntityTable> tables)
        {
            var filterTables = (_Filter?.Tables ?? Enumerable.Empty<EntityTable>());
            var joinTables = _Joins?.SelectMany(j => j.Predicate?.Tables ?? Enumerable.Empty<EntityTable>());
            if (joinTables != null)
                filterTables = filterTables.Concat(joinTables);

            foreach (var t in filterTables)
            {
                tables.Add(t);
            }
        }
        private static bool IsParent(EntityTable parent, EntityTable table)
        {
            while (table.Parent != null)
            {
                if (table.Parent == parent) return true;
                table = table.Parent;
            }

            return false;
        }

        private void CompileJoin(StringBuilder sql, EntityTable table, bool childTable = false)
        {
            sql.AppendLine();
            if (_JoinType == JoinType.LEFT || childTable)
                sql.Append("LEFT ");
            sql.Append("JOIN ")
                .Append(table.TableName)
                .Append(' ');
            table.CompileAlias(sql, _alias)
            .Append(" ON ");
        }

        public IPopertyFilterExpression<T, TProp> FromProp<TProp>(Expression<Func<T, TProp>> expression)
        {
            var memberInfo = Helper.Member(expression);
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);

            return new PopertyFilterExpression<T, TProp>(_alias, entityAttr);
        }

        public IPopertyFilterExpression<T, TProp> FromPropOfType<TProp>(Expression<Func<T, TProp>> expression)
        {
            var memberInfo = Helper.MemberOfTypeCall(expression);
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);

            return new PopertyFilterExpression<T, TProp>(_alias, entityAttr);
        }

        public IJoin<TProp> FetchCollectionJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression, IPathExpression path)
        {
            if (_Joins == null) _Joins = new List<IJoin>();

            var memberInfo = Helper.Member(expression);
            //if (propInfo == null) propInfo = Helper.PropertyOfTypeCall(expression);

            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);
            if (entityAttr == null)
                entityAttr = _ctx.ModelManager.Entity(memberInfo.DeclaringType).GetAttribute(memberInfo.Name);

            int n = _Joins.Count;
            var join = new FetchJoinImpl<TProp>(_ctx, new AliasExpressionImpl(_alias, n.ToString()), path, entityAttr, _Joins);
            
            _Joins.Add(join);

            return join;
        }

        public IJoin<T> FetchCollectionJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression, out IJoin<TProp> alias, IPathExpression path)
        {
            alias = FetchCollectionJoin<TProp>(expression, _path);
            return this;
        }

        public IJoin<TProp> FetchCollectionJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression)
        {
            return FetchCollectionJoin<TProp>(expression, _path);
        }

        public IJoin<T> FetchCollectionJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression, out IJoin<TProp> alias)
        {
            return FetchCollectionJoin<TProp>(expression, out alias, _path);
        }

        public IJoin<TProp> FetchJoin<TProp>(Expression<Func<T, TProp>> expression, IPathExpression path)
        {
            if (_Joins == null) _Joins = new List<IJoin>();

            var memberInfo = Helper.Member(expression);
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);
            if (entityAttr == null)
                entityAttr = _ctx.ModelManager.Entity(memberInfo.DeclaringType).GetAttribute(memberInfo.Name);

            int n = _Joins.Count;
            var join = new FetchJoinImpl<TProp>(_ctx, new AliasExpressionImpl(_alias, n.ToString()), path, entityAttr, _Joins);
            
            _Joins.Add(join);

            return join;
        }

        public IJoin<T> FetchJoin<TProp>(Expression<Func<T, TProp>> expression, out IJoin<TProp> alias, IPathExpression path)
        {
            alias = FetchJoin(expression, path);
            return this;
        }

        public IJoin<TProp> FetchLeftJoin<TProp>(Expression<Func<T, TProp>> expression, IPathExpression path)
        {
            var join = FetchJoin(expression, path);
            join.JoinType = JoinType.LEFT;

            return join;
        }

        public IJoin<T> FetchLeftJoin<TProp>(Expression<Func<T, TProp>> expression, out IJoin<TProp> alias, IPathExpression path)
        {
            alias = FetchJoin(expression, path);
            alias.JoinType = JoinType.LEFT;

            return this;
        }


        public IJoin<TProp> FetchJoin<TProp>(Expression<Func<T, TProp>> expression)
        {
            return FetchJoin<TProp>(expression, _path);
        }

        public IJoin<T> FetchJoin<TProp>(Expression<Func<T, TProp>> expression, out IJoin<TProp> alias)
        {
            return FetchJoin<TProp>(expression, out alias, _path);
        }

        public IJoin<TProp> FetchLeftJoin<TProp>(Expression<Func<T, TProp>> expression)
        {
            return FetchLeftJoin<TProp>(expression, _path);
        }

        public IJoin<T> FetchLeftJoin<TProp>(Expression<Func<T, TProp>> expression, out IJoin<TProp> alias)
        {
            return FetchLeftJoin<TProp>(expression, out alias, _path);
        }

        public IJoin<TProp> Join<TProp>(Expression<Func<T, TProp>> expression)
        {
            if (_Joins == null) _Joins = new List<IJoin>();

            var memberInfo = Helper.Member(expression);
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);

            int n = _Joins.Count;
            var join = new JoinImpl<TProp>(_ctx, new AliasExpressionImpl(_alias, n.ToString()), new PathExpressionImpl(_path, entityAttr.Name), _Joins);
            
            _Joins.Add(join);

            return join;
        }

        public IJoin<TProp> LeftJoin<TProp>(Expression<Func<T, TProp>> expression)
        {
            var join = Join(expression);
            join.JoinType = JoinType.LEFT;

            return join;
        }

        public IJoin<TProp> CollectionJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression)
        {
            if (_Joins == null) _Joins = new List<IJoin>();

            var memberInfo = Helper.Member(expression);
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);

            int n = _Joins.Count;
            var join = new JoinImpl<TProp>(_ctx, new AliasExpressionImpl(_alias, n.ToString()), new PathExpressionImpl(_path, entityAttr.Name), _Joins);
            
            _Joins.Add(join);

            return join;
        }

        public IJoin<TJoin> Join<TJoin>(string alias)
        {
            if (_Joins == null) _Joins = new List<IJoin>();

            //int n = _Joins.Count;
            var join = new JoinImpl<TJoin>(_ctx, new AliasExpressionImpl(alias), new PathExpressionImpl(null, alias), _Joins);
            
            _Joins.Add(join);

            return join;
        }

        public IJoin<TJoin> LeftJoin<TJoin>(string alias)
        {
            var join = Join< TJoin>(alias);
            join.JoinType = JoinType.LEFT;

            return join;
        }


        public IOrderBy<T> OrderBy()
        {
            if (_OrderBy == null)
            {
                _OrderBy = new OrderByImpl<T>(_ctx, _alias);
            }
            return _OrderBy;
        }

        public ISelect<T> Select()
        {
            if (_Select == null)
            {
                _Select = new SelectImpl<T>(_ctx, _alias, _path, null);
            }
            return _Select;
        }

        //public ISelect<TProjection> Select<TProjection>()
        //{
        //    if (_Projection as ISelect<TProjection> == null)
        //    {
        //        _Projection = new SelectImpl<TProjection>(_ctx, _alias, _path/*, _Joins.Select(j => j.SelectExpression())*/);
        //    }
        //    return (ISelect<TProjection>)_Projection;
        //}

        public ISelect<T> Select(IPathExpression path)
        {
            if (_Select == null)
            {
                _Select = new SelectImpl<T>(_ctx, _alias, path, null);
            }
            return _Select;
        }

        public void ClearSelect()
        {
            if (_Select != null) _Select.Clear();
        }

        public Filter<T> Filter()
        {
            return _Filter ?? (_Filter = new FilterImpl<T>(_ctx, _alias));
        }

        public Filter<T> Filter(Func<Filter<T>, Predicate> filterFactory)
        {
            return Filter().Predicate(filterFactory);
        }

        public ISelectExpression SelectExpression()
        {
            return Select();
        }

        public ISelect<T> OfType(IEnumerable<Type> types)
        {
            if (_Select == null)
            {
                _Select = new SelectImpl<T>(_ctx, _alias, _path, types);
            }
            return _Select;
        }

        public ISelect<T> OfType<T1, T2>()
            where T1 : T
            where T2 : T
        {
            return OfType(new[] { typeof(T1), typeof(T2) });
        }

        public ISelect<T> OfType<T1, T2, T3>()
            where T1 : T
            where T2 : T
            where T3 : T
        {
            return OfType(new[] { typeof(T1), typeof(T2), typeof(T3) });
        }

        public IJoin<TProp> FetchCollectionLeftJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression)
        {
            var join = FetchCollectionJoin(expression);
            join.JoinType = JoinType.LEFT;
            return join;
        }

        public IJoin<T> FetchCollectionLeftJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression, out IJoin<TProp> alias)
        {
            FetchCollectionJoin(expression, out alias);
            alias.JoinType = JoinType.LEFT;
            return this;
        }

        public IJoin<TProp> FetchCollectionLeftJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression, IPathExpression mapping)
        {
            var join = FetchCollectionJoin(expression, mapping);
            join.JoinType = JoinType.LEFT;
            return join;
        }

        public IJoin<T> FetchCollectionLeftJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression, out IJoin<TProp> alias, IPathExpression mapping)
        {
            FetchCollectionJoin(expression, out alias, mapping);
            alias.JoinType = JoinType.LEFT;
            return this;
        }

        public IJoin<TOther> OuterApply<TOther>(ISelectQuery<TOther> query, string alias)
        {
            if (_Joins == null) _Joins = new List<IJoin>();

            var join = new ApplyImpl<TOther>(_ctx, new AliasExpressionImpl(alias), null, _Joins, query, false);
            _Joins.Add(join);

            return join;
        }

        public IJoin<TOther> OuterApplyEx<TOther>(ISelectQuery<TOther> query, string alias)
        {
            if (_Joins == null) _Joins = new List<IJoin>();

            var join = new ApplyImplEx<TOther>(_ctx, new AliasExpressionImpl(alias), new PathExpressionImpl(null, alias), _Joins, query, false);
            _Joins.Add(join);

            return join;
        }

        public IJoin<TOther> CrossApply<TOther>(ISelectQuery<TOther> query, string alias)
        {
            if (_Joins == null) _Joins = new List<IJoin>();

            var join = new ApplyImpl<TOther>(_ctx, new AliasExpressionImpl(alias), null, _Joins, query, true);
            _Joins.Add(join);

            return join;
        }

        public IJoin<TOther> CrossApplyEx<TOther>(ISelectQuery<TOther> query, string alias)
        {
            if (_Joins == null) _Joins = new List<IJoin>();

            var join = new ApplyImplEx<TOther>(_ctx, new AliasExpressionImpl(alias), new PathExpressionImpl(null, alias), _Joins, query, true);
            _Joins.Add(join);

            return join;
        }
    }

    class ApplyImpl<T> : JoinImpl<T>
    {
        ISelectQuery<T> _query;
        bool _isCrossApply;

        public ApplyImpl(QueryContext ctx, IAliasExpression alias, IPathExpression path, List<IJoin> joins, ISelectQuery<T> query, bool isCrossApply) : 
            base(ctx, alias, path, joins) 
        { 
            _query = query;
            _isCrossApply = isCrossApply;

            _Filter = query.Filter();
        }

        public override StringBuilder CompileToSQL(StringBuilder sql)
        {
            sql.AppendLine().AppendLine(_isCrossApply ? " CROSS APPLY (" : "OUTER APPLY (");

            _query.CompileToSQL(sql);

            sql.AppendLine().Append(") ");
            _alias.CompileToSQL(sql);

            return sql;

        }
    }

    class ApplyImplEx<T> : ApplyImpl<T>
    {
        public ApplyImplEx(QueryContext ctx, IAliasExpression alias, IPathExpression path, List<IJoin> joins, ISelectQuery<T> query, bool isCrossApply) :
            base(ctx, alias, path, joins, query, isCrossApply)
        {
            _Select = new SelectImpl<T>(_ctx, _alias, _path, null);

            foreach (var expr in query.SelectExpression().Expressions())
                _Select.Select(new SelectAliasExpressionImpl(alias, expr.EntityAttribute, path));

            var selectSubQueryImpl =  (SelectQueryImpl<T>)query;
            var subQueryJoins = selectSubQueryImpl.Joins();
            if (subQueryJoins == null) return;

            foreach(var join in subQueryJoins)
            {
                var joinPath = new PathExpressionImpl(path, join.Path);
                
                foreach ( var expr in join.SelectExpression().Expressions())
                    _Select.Select(new SelectSubQueryAliasExpressionImpl(alias, expr.EntityAttribute, join.Path, joinPath)); // new PathExpressionImpl(joinPath, expr.EntityAttribute.Name)

            }
        }

    }
}
