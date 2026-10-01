namespace NashunumaApp.Infrastructure.Helpers
{
    public static class BatchUploadLimits
    {
        //Max batches allowed in a single file upload
        public const int MaxBatchesPerFile = 5000;
        //Max batches returned as inline JSON (no pagination).
        public const int InlineJsonMaxBatches = 200;

        //Max file size for upload (10 MB).
        public const long MaxFileSizeBytes = 10 * 1024 * 1024;

        //Chunk size for SQL IN(...) queries.
        public const int SqlInChunkSize = 500;
    }
}