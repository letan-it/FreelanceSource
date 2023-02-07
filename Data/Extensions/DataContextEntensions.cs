using Core;
using Core.Data;
using Core.Data.Attributes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Data.Extensions
{
    public static class DataContextEntensions
    {
        private static string databaseName;
        private static readonly ConcurrentDictionary<string, string> tableNames = new ConcurrentDictionary<string, string>();


        /// <summary>
        /// Get SQL commands from the script
        /// </summary>
        /// <param name="sql">SQL script</param>
        /// <returns>List of commands</returns>
        private static IList<string> GetCommandsFromScript(string sql)
        {
            var commands = new List<string>();

            //origin from the Microsoft.EntityFrameworkCore.Migrations.SqlServerMigrationsSqlGenerator.Generate method
            sql = Regex.Replace(sql, @"\\\r?\n", string.Empty);
            var batches = Regex.Split(sql, @"^\s*(GO[ \t]+[0-9]+|GO)(?:\s+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            for (var i = 0; i < batches.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(batches[i]) || batches[i].StartsWith("GO", StringComparison.OrdinalIgnoreCase))
                    continue;

                var count = 1;
                if (i != batches.Length - 1 && batches[i + 1].StartsWith("GO", StringComparison.OrdinalIgnoreCase))
                {
                    var match = Regex.Match(batches[i + 1], "([0-9]+)");
                    if (match.Success)
                        count = int.Parse(match.Value);
                }

                var builder = new StringBuilder();
                for (var j = 0; j < count; j++)
                {
                    builder.Append(batches[i]);
                    if (i == batches.Length - 1)
                        builder.AppendLine();
                }

                commands.Add(builder.ToString());
            }

            return commands;
        }

        /// <summary>
        /// Get table name of entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="context">Database context</param>
        /// <returns>Table name</returns>
        //public static string GetTableName<TEntity>(this IDbContext context) where TEntity : BaseEntity
        //{
        //    if (context == null)
        //        throw new ArgumentNullException(nameof(context));

        //    //try to get the EF database context
        //    if (!(context is DbContext dbContext))
        //        throw new InvalidOperationException("Context does not support operation");

        //    var entityTypeFullName = typeof(TEntity).FullName;
        //    if (!tableNames.ContainsKey(entityTypeFullName))
        //    {
        //        //get entity type
        //        var entityType = dbContext.Model.FindRuntimeEntityType(typeof(TEntity));

        //        //get the name of the table to which the entity type is mapped
        //        tableNames.TryAdd(entityTypeFullName, entityType.Relational().TableName);
        //    }

        //    tableNames.TryGetValue(entityTypeFullName, out var tableName);

        //    return tableName;
        //}
        /// <summary>
        /// Get database name
        /// </summary>
        /// <param name="context">Database context</param>
        /// <returns>Database name</returns>
        public static string DbName(this IDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            //try to get the EF database context
            if (!(context is DbContext dbContext))
                throw new InvalidOperationException("Context does not support operation");

            if (!string.IsNullOrEmpty(databaseName))
                return databaseName;

            //get database connection
            var dbConnection = dbContext.Database.GetDbConnection();

            //return the database name
            databaseName = dbConnection.Database;

            return databaseName;
        }
        /// <summary>
        /// Execute commands from the SQL script against the context database
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="sql">SQL script</param>
        public static void ExecuteSqlScript(this IDbContext context, string sql)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var sqlCommands = GetCommandsFromScript(sql);
            foreach (var command in sqlCommands)
                context.ExecuteSqlCommand(command);
        }

        /// <summary>
        /// Execute commands from a file with SQL script against the context database
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="filePath">Path to the file</param>
        public static void ExecuteSqlScriptFromFile(this IDbContext context, string filePath)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!File.Exists(filePath))
                return;

            context.ExecuteSqlScript(File.ReadAllText(filePath));
        }

        public static DataContext As(this IDbContext context)
        {
            return (DataContext)context;
        }
        public static FunctionInfo GetCommandFromMethod(this IDbContext context, MethodInfo methed, params object[] parameterValues)
        {

            FunctionInfo function = new FunctionInfo();
            try
            {
                var Function = methed.GetCustomAttributes(typeof(FunctionAttribute));
                if (Function != null && Function.Any())
                {
                    var func = (FunctionAttribute)Function.FirstOrDefault();
                    function.Query = func.Name;
                    var Parameters = methed.GetParameters();
                    if (Parameters != null && Parameters.Any())
                    {
                        function.Parameters = new System.Data.Common.DbParameter[Parameters.Count()];
                        int i = 0;
                        foreach (var item in Parameters)
                        {
                            var param = item.GetCustomAttribute<ParameterAttribute>();
                            var p = new EfDataProviderManager().DataProvider.GetParameter();
                            var ParameterName = "";
                            if (param != null)
                            {
                                ParameterName = param.Name;
                            }
                            else
                            {
                                ParameterName = item.Name;
                            }
                            p.ParameterName = ParameterName;
                            p.Value = parameterValues[item.Position];
                            if (item.ParameterType.IsByRef)
                            {
                                p.Direction = ParameterDirection.InputOutput;
                            }
                            else if (item.IsOut)
                            {
                                p.Direction = ParameterDirection.Output;
                            }
                            else
                            {
                                p.Direction = ParameterDirection.Input;
                            }
                            function.Parameters[i] = p;
                            i++;
                        }
                    }
                    /*
                    var returnParameter = cmd.CreateParameter();
                    returnParameter.ParameterName = "@return_value";
                    returnParameter.DbType = DbType.Int32;
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(returnParameter);
                    */
                    return function;

                }
                throw new ArgumentNullException("Function", "Function name is not entry.");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
