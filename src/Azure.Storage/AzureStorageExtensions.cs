using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Microsoft.WindowsAzure.Storage.Table
{
    /// <summary>
    /// AzureStorageExtensions.
    /// </summary>
    /// TODO Edit XML Comment Template for AzureStorageExtensions
    public static class AzureStorageExtensions
    {
        /// <summary>
        /// Executes the query asynchronous.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <returns>Task{IEnumerable{DynamicTableEntity}}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQueryAsync
        public static Task<IEnumerable<DynamicTableEntity>> ExecuteQueryAsync(this CloudTable table,TableQuery query)
        {
            return ExecuteQueryAsync(table, query, CancellationToken.None);
        }

        /// <summary>
        /// execute query as an asynchronous operation.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="requestOptions"></param>
        /// <returns>Task{IEnumerable{T}}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQueryAsync`1
        public static Task<IEnumerable<DynamicTableEntity>> ExecuteQueryAsync(this CloudTable table, TableQuery query, TableRequestOptions requestOptions)
        {
            return ExecuteQueryAsync(table, query, requestOptions, null, CancellationToken.None);
        }

        /// <summary>
        /// execute query as an asynchronous operation.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="requestOptions"></param>
        /// <param name="operationContext"></param>
        /// <returns>Task{IEnumerable{T}}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQueryAsync`1
        public static Task<IEnumerable<DynamicTableEntity>> ExecuteQueryAsync(this CloudTable table, TableQuery query, TableRequestOptions requestOptions, OperationContext operationContext)
        {
            return ExecuteQueryAsync(table, query, requestOptions, operationContext, CancellationToken.None);
        }

        /// <summary>
        /// execute query as an asynchronous operation.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{IEnumerable{T}}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQueryAsync`1
        public static Task<IEnumerable<DynamicTableEntity>> ExecuteQueryAsync(this CloudTable table, TableQuery query, CancellationToken cancellationToken)
        {
            return ExecuteQueryAsync(table, query, null, null, cancellationToken);
        }

        /// <summary>
        /// execute query as an asynchronous operation.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{IEnumerable{T}}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQueryAsync`1
        public static Task<IEnumerable<DynamicTableEntity>> ExecuteQueryAsync(this CloudTable table, TableQuery query, TableRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return ExecuteQueryAsync(table, query, requestOptions, null, cancellationToken);
        }

        /// <summary>
        /// execute query as an asynchronous operation.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="requestOptions"></param>
        /// <param name="operationContext"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{IEnumerable{DynamicTableEntity}}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQueryAsync
        public static async Task<IEnumerable<DynamicTableEntity>> ExecuteQueryAsync(this CloudTable table, TableQuery query, TableRequestOptions requestOptions, OperationContext operationContext, CancellationToken cancellationToken)
        {
            TableContinuationToken continuationToken = null;
            var results = new List<DynamicTableEntity>();
            do
            {
                var querySegment = await table.ExecuteQuerySegmentedAsync(query, continuationToken, requestOptions, operationContext, cancellationToken).ConfigureAwait(false);
                results.AddRange(querySegment);
                continuationToken = querySegment.ContinuationToken;
            } while (continuationToken != null && !cancellationToken.IsCancellationRequested);

            return results;
        }

        /// <summary>
        /// Observes the query.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <returns>IObservable{DynamicTableEntity}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQuery
        public static IObservable<DynamicTableEntity> ObserveQuery(this CloudTable table, TableQuery query)
        {
            return ObserveQuery(table, query, null, null);
        }

        /// <summary>
        /// Observes the query.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="requestOptions"></param>
        /// <returns>IObservable{DynamicTableEntity}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQuery
        public static IObservable<DynamicTableEntity> ObserveQuery(this CloudTable table, TableQuery query, TableRequestOptions requestOptions)
        {
            return ObserveQuery(table, query, requestOptions, null);
        }

        /// <summary>
        /// Observes the query.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="requestOptions"></param>
        /// <param name="operationContext"></param>
        /// <returns>IObservable{DynamicTableEntity}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQuery
        public static IObservable<DynamicTableEntity> ObserveQuery(this CloudTable table, TableQuery query, TableRequestOptions requestOptions, OperationContext operationContext)
        {
            return Observable.Create<DynamicTableEntity>(async (o, cancellationToken) =>
            {
                TableContinuationToken continuationToken = null;
                do
                {
                    var querySegment = await table.ExecuteQuerySegmentedAsync(query, continuationToken, requestOptions, operationContext, cancellationToken).ConfigureAwait(false);
                    foreach (var item in querySegment)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;
                        o.OnNext(item);
                    }
                    continuationToken = querySegment.ContinuationToken;
                } while (continuationToken != null && !cancellationToken.IsCancellationRequested);

                o.OnCompleted();
            });
        }

        /// <summary>
        /// Executes the query asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <returns>Task{IEnumerable{T}}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQueryAsync`1
        public static Task<IEnumerable<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query)
            where T : ITableEntity, new()
        {
            return ExecuteQueryAsync(table, query, CancellationToken.None);
        }

        /// <summary>
        /// execute query as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="requestOptions"></param>
        /// <returns>Task{IEnumerable{T}}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQueryAsync`1
        public static Task<IEnumerable<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query, TableRequestOptions requestOptions)
            where T : ITableEntity, new()
        {
            return ExecuteQueryAsync(table, query, requestOptions, null, CancellationToken.None);
        }

        /// <summary>
        /// execute query as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="requestOptions"></param>
        /// <param name="operationContext"></param>
        /// <returns>Task{IEnumerable{T}}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQueryAsync`1
        public static  Task<IEnumerable<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query, TableRequestOptions requestOptions, OperationContext operationContext)
            where T : ITableEntity, new()
        {
            return ExecuteQueryAsync(table, query, requestOptions, operationContext, CancellationToken.None);
        }

        /// <summary>
        /// execute query as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{IEnumerable{T}}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQueryAsync`1
        public static Task<IEnumerable<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query, CancellationToken cancellationToken)
            where T : ITableEntity, new()
        {
            return ExecuteQueryAsync(table, query, null, null, cancellationToken);
        }

        /// <summary>
        /// execute query as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{IEnumerable{T}}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQueryAsync`1
        public static Task<IEnumerable<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query, TableRequestOptions requestOptions, CancellationToken cancellationToken)
            where T : ITableEntity, new()
        {
            return ExecuteQueryAsync(table, query, requestOptions, null, cancellationToken);
        }

        /// <summary>
        /// execute query as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="operationContext"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="requestOptions"></param>
        /// <returns>Task{IEnumerable{T}}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQueryAsync`1
        public static async Task<IEnumerable<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query, TableRequestOptions requestOptions, OperationContext operationContext, CancellationToken cancellationToken)
            where T : ITableEntity, new()
        {
            TableContinuationToken continuationToken = null;
            var results = new List<T>();
            do
            {
                var querySegment = await table.ExecuteQuerySegmentedAsync(query, continuationToken, null, null, cancellationToken).ConfigureAwait(false);
                results.AddRange(querySegment);
                continuationToken = querySegment.ContinuationToken;
            } while (continuationToken != null && !cancellationToken.IsCancellationRequested);

            return results;
        }

        /// <summary>
        /// Observes the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <returns>IObservable{T}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQuery`1
        public static IObservable<T> ObserveQuery<T>(this CloudTable table, TableQuery<T> query)
            where T : ITableEntity, new()
        {
            return ObserveQuery(table, query, null, null);
        }

        /// <summary>
        /// Observes the query.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="requestOptions"></param>
        /// <returns>IObservable{DynamicTableEntity}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQuery
        public static IObservable<T> ObserveQuery<T>(this CloudTable table, TableQuery<T> query, TableRequestOptions requestOptions)
            where T : ITableEntity, new()
        {
            return ObserveQuery(table, query, requestOptions, null);
        }

        /// <summary>
        /// Observes the query.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="query">The query.</param>
        /// <param name="requestOptions"></param>
        /// <param name="operationContext"></param>
        /// <returns>IObservable{DynamicTableEntity}.</returns>
        /// TODO Edit XML Comment Template for ExecuteQuery
        public static IObservable<T> ObserveQuery<T>(this CloudTable table, TableQuery<T> query, TableRequestOptions requestOptions, OperationContext operationContext)
            where T : ITableEntity, new()
        {
            return Observable.Create<T>(async (o, cancellationToken) =>
            {
                TableContinuationToken continuationToken = null;
                do
                {
                    var querySegment = await table.ExecuteQuerySegmentedAsync(query, continuationToken, requestOptions, operationContext, cancellationToken).ConfigureAwait(false);
                    foreach (var item in querySegment)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;
                        o.OnNext(item);
                    }
                    continuationToken = querySegment.ContinuationToken;
                } while (continuationToken != null && !cancellationToken.IsCancellationRequested);

                o.OnCompleted();
            });
        }
    }
}
