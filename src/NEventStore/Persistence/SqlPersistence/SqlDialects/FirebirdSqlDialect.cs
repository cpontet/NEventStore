namespace NEventStore.Persistence.SqlPersistence.SqlDialects
{
    using System.Data;
    using System.Transactions;

    public class FirebirdSqlDialect : CommonSqlDialect
    {
        public override string InitializeStorage
        {
            get { return FirebirdSqlStatements.InitializeStorage; }
        }

        public override string AppendSnapshotToCommit
        {
            get { return base.AppendSnapshotToCommit.Replace("/*FROM DUAL*/", "FROM rdb$database"); }
        }

        public override string GetSnapshot
        {
            get { return base.GetSnapshot.Replace("SELECT *", "SELECT FIRST 1 *").Replace("LIMIT 1", string.Empty); }
        }

        public override string GetCommitsFromStartingRevision
        {
            get { return Paged(base.GetCommitsFromStartingRevision); }
        }

        public override string GetCommitsFromInstant
        {
            get { return Paged(base.GetCommitsFromInstant); }
        }

        public override string GetCommitsFromToInstant
        {
            get { return Paged(base.GetCommitsFromToInstant); }
        }

        public override string GetStreamsRequiringSnapshots
        {
            get { return Paged(base.GetStreamsRequiringSnapshots); }
        }

        public override string GetUndispatchedCommits
        {
            get { return Paged(base.GetUndispatchedCommits); }
        }

        private string Paged(string query)
        {
            if (query.Contains(Skip))
            {
                return query.Replace("SELECT ", "SELECT FIRST @Limit SKIP @Skip ").Replace("\n LIMIT @Limit OFFSET @Skip;", ";");
            }

            return query.Replace("SELECT ", "SELECT FIRST @Limit ").Replace("\n LIMIT @Limit;", ";");
        }

        public override IDbStatement BuildStatement(
            TransactionScope scope, IDbConnection connection, IDbTransaction transaction)
        {
            return new FirebirdDbStatement(this, scope, connection, transaction);
        }

        private class FirebirdDbStatement : DelimitedDbStatement
        {
            public FirebirdDbStatement(
                ISqlDialect dialect,
                TransactionScope scope,
                IDbConnection connection,
                IDbTransaction transaction)
                : base(dialect, scope, connection, transaction)
            {}
        }
    }
}