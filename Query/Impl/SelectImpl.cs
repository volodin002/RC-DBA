using RC.DBA.Metamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Query.Impl
{
    class SelectImpl<T> : ISelect<T>
    {
        List<ISelectExpression> _expressions;
        
        QueryContext _ctx;
        IAliasExpression _alias;
        IPathExpression _path;

        SelectDescriminatorExpressionImpl _descriminator;
        private HashSet<Type> _onlyTypes;

        public SelectImpl(QueryContext ctx, IAliasExpression alias, IPathExpression path, IEnumerable<Type> onlyTypes)
        {
            _alias = alias;
            _ctx = ctx;
            _path = path;
            _expressions = new List<ISelectExpression>();

            if (onlyTypes != null)
                _onlyTypes = new HashSet<Type>(onlyTypes);
            //if (_ctx.ModelManager.Entity<T>().IsAbstract)
            //{
            //    SelectDescriminator();
            //}

        }

        public StringBuilder CompileToSQL(StringBuilder sql)
        {
            if (_expressions != null && _expressions.Count > 0)
            {
                _expressions[0].CompileToSQL(sql);
                for (int i = 1; i < _expressions.Count; i++)
                {
                    sql.Append(',');
                    _expressions[i].CompileToSQL(sql);
                }
            }

            return sql;
        }

        public IEnumerable<ISelectExpression> Expressions()
        {
            return _expressions;
        }

        public ISelect<T> Select<TProp>(ISelectExpression<T, TProp> expression)
        {
            _expressions.Add(expression);
            EnsureDescriminator();
            return this;
        }

        //public ISelect<T> Select<TProp>(Action<StringBuilder, Func<StringBuilder, StringBuilder>> compile, Expression<Func<T, TProp>> expression)
        //{
        //    var propInfo = Helper.Property(expression);
        //    var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(propInfo.Name);

        //    _expressions.Add(new SelectExpressionImplFunc<T, TProp>(compile, _alias, entityAttr, _path));
        //    EnsureDescriminator();
        //    return this;
        //}

        public ISelectExpression Select(ISelectExpression expression)
        {
            _expressions.Add(expression);
            EnsureDescriminator();
            return this;
        }

        public ISelect<T> Select<TProp>(Expression<Func<T, TProp>> expression)
        {
            var memberInfo = Helper.Member(expression);
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);

            _expressions.Add(new SelectExpressionImpl<T, TProp>(_alias, entityAttr, _path));
            EnsureDescriminator();
            return this;
        }
        

        public ISelect<T> SelectAll()
        {
            var entity = _ctx.ModelManager.Entity<T>();
            var attributes = entity.AllAttributes;

            _expressions = attributes.Where(a => !a.IsNotMapped && !a.IsAssociation)
                .Select(a => new SelectExpressionImpl(_alias, a, _path))
                .OfType<ISelectExpression>().ToList();

            if (_descriminator != null)
            {
                _expressions.Add(_descriminator);
            }
            else if (entity.IsAbstract)
            {
                SelectDescriminator();
            }

            return this;
        }

        public ISelect<T> SelectAllFromType<Tx>() where Tx : T
        {
            var entity = _ctx.ModelManager.Entity<Tx>();
            var attributes = entity.Attributes;

            _expressions.AddRange(attributes.Where(a => !a.IsNotMapped && !a.IsAssociation)
                .Select(a => new SelectExpressionImpl(_alias, a, _path) { AddClassName = true }) 
                .OfType<ISelectExpression>());

            return this;
        }

        public ISelect<T> SelectFromType<Tx, TProp>(Tp<Tx> tx, Expression<Func<Tx, TProp>> expression) where Tx : T
        {

            var memberInfo = Helper.Member(expression);
            var entityAttr = _ctx.ModelManager.Entity<Tx>().GetAttribute(memberInfo.Name);

            _expressions.Add(new SelectExpressionImpl<T, TProp>(_alias, entityAttr, _path) { AddClassName = true });
            EnsureDescriminator();

            return this;
        }

        public ISelect<T> SelectDescriminator()
        {
            var manager = _ctx.ModelManager;
            var entity = manager.Entity<T>();
            
            
            //if (entity.Implementations == null)
            //    entity.Implementations = manager.GetImplementations<T>();

            var implementations = entity.Implementations.ToArray();
            if (implementations.Length == 0)
            {
                if (!entity.IsAbstract)
                    implementations = new[] { entity };
                else
                    throw new Exception("Cannot define Descriminator!");
            }

            if (_onlyTypes != null)
            {
                implementations = implementations
                    .Where(impl => _onlyTypes.Contains(impl.ClassType))
                    .ToArray();
            }
            _descriminator = new SelectDescriminatorExpressionImpl(_alias, _path, implementations);
            _expressions.Add(_descriminator);

            return this;
        }

        public IEnumerable<EntityTable> Tables()
        {
            return _expressions.SelectMany(exp => exp.Tables()).Distinct();
        }

        protected void EnsureDescriminator()
        {
            if (_descriminator == null && _ctx.ModelManager.Entity<T>().IsAbstract)
            {
                SelectDescriminator();
            }
        }
    }

    //class SelectExpressionImplFunc<T, TProp> : SelectExpressionImpl, ISelectExpression<T, TProp>
    //{
    //    Action<StringBuilder, Func<StringBuilder, StringBuilder>> _compile;
    //    public SelectExpressionImplFunc(Action<StringBuilder, Func<StringBuilder, StringBuilder>> compile, IAliasExpression alias, IEntityAttribute entityAttribute, IPathExpression path)
    //        : base(alias, entityAttribute, path)
    //    {
    //        _compile = compile;
    //    }

    //    public override StringBuilder CompileToSQL(StringBuilder sql)
    //    {
    //        _compile(sql, x => base.CompileToSQL(x));
    //        return sql;
    //    }
    //}

    class SelectExpressionImpl<T, TProp> : SelectExpressionImpl, ISelectExpression<T, TProp>
    {
        public SelectExpressionImpl(IAliasExpression alias, IEntityAttribute entityAttribute, IPathExpression path)
            :base(alias, entityAttribute, path)
        { }
    }

    class SelectExpressionImpl : ISelectExpression
    {
        IAliasExpression _alias;
        IEntityAttribute _entityAttribute;
        IPathExpression _path;

        private bool _AddClassName;
        public bool AddClassName { get => _AddClassName; set => _AddClassName = value; }

        public SelectExpressionImpl(IAliasExpression alias, IEntityAttribute entityAttribute, IPathExpression path)
        {
            _alias = alias;
            _entityAttribute = entityAttribute;
            _path = path;
        }
        public virtual StringBuilder CompileToSQL(StringBuilder sql)
        {
            sql.AppendLine();
            _entityAttribute.EntityType.Table
                .CompileAlias(sql, _alias)
                .Append('.').Append(_entityAttribute.Name)
                .Append(" ");

            sql.Append('[');
            if (_path != null)
            {
                _path.CompileToSQL(sql).Append('.');
            }
            if(_AddClassName)
            {
                sql.Append(_entityAttribute.EntityType.ClassType.Name).Append('$');
            }
            return sql.Append(_entityAttribute.Name).Append(']');
        }

        public IEnumerable<ISelectExpression> Expressions()
        {
            yield return this;

        }

        public IEnumerable<EntityTable> Tables()
        {
            yield return _entityAttribute.EntityType.Table;
        }

    }

    class SelectDescriminatorExpressionImpl : ISelectExpression
    {
        IAliasExpression _alias;
        IPathExpression _path;
        IEntityType[] _implementations;

        public SelectDescriminatorExpressionImpl(IAliasExpression alias, IPathExpression path, IEntityType[] implementations)
        {
            _alias = alias;
            _path = path;
            _implementations = implementations;
        }

        public StringBuilder CompileToSQL(StringBuilder sql)
        {
            sql.AppendLine();
            sql.Append("CASE ");
            for (int i = 0; i < _implementations.Length; i++)
            {
                sql.Append("WHEN ");
                _implementations[i].Table.CompileAlias(sql, _alias);
                sql.Append('.').Append(_implementations[i].Key.Column).Append(" Is not NULL THEN ");
                sql.Append(_implementations[i].Descriminator).Append(' ');
            }
            sql.Append(" END ");

            sql.Append('[');
            if (_path != null)
            {
                _path.CompileToSQL(sql).Append('.');
            }
            return sql.Append("$type").Append(']');
        }

        public IEnumerable<ISelectExpression> Expressions()
        {
            yield return this;
        }

        public IEnumerable<EntityTable> Tables()
        {
            return _implementations.Select(i => i.Table).Distinct();
        }
    }

}
