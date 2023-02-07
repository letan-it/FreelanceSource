using Core;
using Core.Data;
using Data.Extensions;
using Data.Mapping;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Data
{
    public class DataContext : DbContext, IDbContext
    {

        #region Ctor

        protected DataContext()
        {

        }
        public string ConnectionString = null;
        public DataContext(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrWhiteSpace(ConnectionString))
            {
                optionsBuilder.UseSqlServer(ConnectionString);
            }
            base.OnConfiguring(optionsBuilder);
        }
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        #endregion
        #region Utilities

        /// <summary>
        /// Further configuration the model
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //dynamically load all entity and query type configurations
            var typeConfigurations = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
                (type.BaseType?.IsGenericType ?? false)
                    && (type.BaseType.GetGenericTypeDefinition() == typeof(DiEntityTypeConfiguration<>)
                        || type.BaseType.GetGenericTypeDefinition() == typeof(DiQueryTypeConfiguration<>)));

            foreach (var typeConfiguration in typeConfigurations)
            {
                var configuration = (IMappingConfiguration)Activator.CreateInstance(typeConfiguration);
                configuration.ApplyConfiguration(modelBuilder);
            }

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Modify the input SQL query by adding passed parameters
        /// </summary>
        /// <param name="sql">The raw SQL query</param>
        /// <param name="parameters">The values to be assigned to parameters</param>
        /// <returns>Modified raw SQL query</returns>
        protected virtual string CreateSqlWithParameters(string sql, params object[] parameters)
        {
            //add parameters to sql
            for (var i = 0; i <= (parameters?.Length ?? 0) - 1; i++)
            {
                if (!(parameters[i] is DbParameter parameter))
                    continue;

                sql = $"{sql}{(i > 0 ? "," : string.Empty)} @{parameter.ParameterName}";

                //whether parameter is output
                if (parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Output)
                    sql = $"{sql} output";
            }

            return sql;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a DbSet that can be used to query and save instances of entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>A set for the given entity type</returns>
        public virtual new DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        /// <summary>
        /// Generate a script to create all tables for the current model
        /// </summary>
        /// <returns>A SQL script</returns>
        public virtual string GenerateCreateScript()
        {
            return this.Database.GenerateCreateScript();
        }

        /// <summary>
        /// Creates a LINQ query for the query type based on a raw SQL query
        /// </summary>
        /// <typeparam name="TQuery">Query type</typeparam>
        /// <param name="sql">The raw SQL query</param>
        /// <returns>An IQueryable representing the raw SQL query</returns>
        //public virtual IQueryable<TQuery> QueryFromSql<TQuery>(string sql) where TQuery : class
        //{
        //    return this.Query<TQuery>().FromSql(sql);
        //}

        /// <summary>
        /// Creates a LINQ query for the entity based on a raw SQL query
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="sql">The raw SQL query</param>
        /// <param name="parameters">The values to be assigned to parameters</param>
        /// <returns>An IQueryable representing the raw SQL query</returns>
        //public virtual IQueryable<TEntity> EntityFromSql<TEntity>(string sql, params object[] parameters) where TEntity : BaseEntity
        //{
        //    return this.Set<TEntity>().FromSql(CreateSqlWithParameters(sql, parameters), parameters);
        //}

        /// <summary>
        /// Executes the given SQL against the database
        /// </summary>
        /// <param name="sql">The SQL to execute</param>
        /// <param name="doNotEnsureTransaction">true - the transaction creation is not ensured; false - the transaction creation is ensured.</param>
        /// <param name="timeout">The timeout to use for command. Note that the command timeout is distinct from the connection timeout, which is commonly set on the database connection string</param>
        /// <param name="parameters">Parameters to use with the SQL</param>
        /// <returns>The number of rows affected</returns>
        public virtual int ExecuteSqlCommand(RawSqlString sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            //set specific command timeout
            var previousTimeout = this.Database.GetCommandTimeout();
            this.Database.SetCommandTimeout(timeout);

            var result = 0;
            if (!doNotEnsureTransaction)
            {
                //use with transaction
                using (var transaction = this.Database.BeginTransaction())
                {
                    result = this.Database.ExecuteSqlCommand(sql, parameters);
                    transaction.Commit();
                }
            }
            else
                result = this.Database.ExecuteSqlCommand(sql, parameters);

            //return previous timeout back
            this.Database.SetCommandTimeout(previousTimeout);

            return result;
        }

        /// <summary>
        /// Detach an entity from the context
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        public virtual void Detach<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entityEntry = this.Entry(entity);
            if (entityEntry == null)
                return;

            //set the entity is not being tracked by the context
            entityEntry.State = EntityState.Detached;
        }

        public int ExecuteNonQuery(bool doNotEnsureTransaction, int? timeout, MethodInfo methed, params object[] parameterValues)
        {
            var conn = this.Database.GetDbConnection();

            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                int? Timeout = this.Database.GetCommandTimeout();
                if (timeout != null)
                {
                    Timeout = timeout;
                }

                int result = -1;

                var func = this.GetCommandFromMethod(methed, parameterValues);


                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = func.Query;
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (func.Parameters != null)
                        cmd.Parameters.AddRange(func.Parameters);
                    if (Timeout != null)
                        cmd.CommandTimeout = Timeout.Value;
                    if (!doNotEnsureTransaction)
                    {
                        using (var dbTransaction = conn.BeginTransaction())
                        {
                            try
                            {
                                cmd.Transaction = dbTransaction;
                                result = cmd.ExecuteNonQuery();
                                dbTransaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                if (!doNotEnsureTransaction)
                                {
                                    dbTransaction.Rollback();
                                }
                                throw ex;
                            }
                        }
                    }
                    else
                        result = cmd.ExecuteNonQuery();
                }
                return result;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
            }

        }


        //public IQueryable<TQuery> QueryFromSql<TQuery>(MethodInfo methed, params object[] parameterValues) where TQuery : class
        //{
        //    var cmd = this.GetCommandFromMethod(methed, parameterValues);
        //    return this.Query<TQuery>().FromSql(CreateSqlWithParameters(cmd.Query, cmd.Parameters), cmd.Parameters);
        //}
        public DataTable ExecuteDatatable(MethodInfo methed, params object[] parameterValues)
        {
            var dt = new DataTable();
            var conn = this.Database.GetDbConnection();

            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                int? Timeout = this.Database.GetCommandTimeout();
                var func = this.GetCommandFromMethod(methed, parameterValues);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = func.Query;
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (func.Parameters != null)
                        cmd.Parameters.AddRange(func.Parameters);
                    if (Timeout != null)
                        cmd.CommandTimeout = Timeout.Value;
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }

                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
            }

            return dt;
        }
        public DataSet ExecuteDataset(MethodInfo methed, params object[] parameterValues)
        {
            var ds = new DataSet();
            var conn = this.Database.GetDbConnection();

            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                int? Timeout = this.Database.GetCommandTimeout();
                var func = this.GetCommandFromMethod(methed, parameterValues);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = func.Query;
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (func.Parameters != null)
                        cmd.Parameters.AddRange(func.Parameters);
                    if (Timeout != null)
                        cmd.CommandTimeout = Timeout.Value;
                    using (DbDataAdapter dataAdapter = new SqlDataAdapter((SqlCommand)cmd))
                    {
                        dataAdapter.Fill(ds);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
            }

            return ds;
        }

        public DataSet ExecuteQuery(string Query)
        {
            var ds = new DataSet();
            var conn = this.Database.GetDbConnection();

            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                int? Timeout = this.Database.GetCommandTimeout();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = Query;


                    if (Timeout != null)
                        cmd.CommandTimeout = Timeout.Value;
                    using (DbDataAdapter dataAdapter = new SqlDataAdapter((SqlCommand)cmd))
                    {
                        dataAdapter.Fill(ds);
                    }
                }
            }
            catch (Exception e)
            {

                throw e;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
            }

            return ds;
        }
        public ExecuteResult ExecuteResult(MethodInfo methed, params object[] parameterValues)
        {
            var result = new ExecuteResult();
            var conn = this.Database.GetDbConnection();

            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                int? Timeout = this.Database.GetCommandTimeout();
                var func = this.GetCommandFromMethod(methed, parameterValues);

                var dt = new DataTable();
                using (var cmd = conn.CreateCommand())
                {

                    cmd.CommandText = func.Query;
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (func.Parameters != null)
                        cmd.Parameters.AddRange(func.Parameters);
                    if (Timeout != null)
                        cmd.CommandTimeout = Timeout.Value;
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    result.Result = dt;
                    foreach (IDataParameter item in cmd.Parameters)
                    {
                        if (item.Direction == ParameterDirection.Output || item.Direction == ParameterDirection.InputOutput)
                        {
                            result.Outputs.Add(item.ParameterName, item.Value);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
            }

            return result;
        }

        public int ExecuteNonQuery(MethodInfo methed, params object[] parameterValues)
        {
            return ExecuteNonQuery(true, null, methed, parameterValues);
        }

        public IQueryable<TQuery> QueryFromSql<TQuery>(string sql) where TQuery : class
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> EntityFromSql<TEntity>(string sql, params object[] parameters) where TEntity : BaseEntity
        {
            throw new NotImplementedException();
        }

        public IQueryable<TQuery> QueryFromSql<TQuery>(MethodInfo methed, params object[] parameterValues) where TQuery : class
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    
}
