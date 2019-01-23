using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json.Linq;

namespace Doobry.Features.QueryDeveloper
{
    public class ResultSet
    {        
        private readonly ObservableCollection<Result> _results;
        private readonly ObservableCollection<FeedResponse<dynamic>> _resultMetadatas;

        public ResultSet(IEnumerable<Result> results)
        {            
            if (results == null) throw new ArgumentNullException(nameof(results));

            _results = new ObservableCollection<Result>(results);
            Results = new ReadOnlyObservableCollection<Result>(_results);            
        }

        public ResultSet(IEnumerable<Result> results, FeedResponse<dynamic> resultMetadatas)
        {
            if (results == null) throw new ArgumentNullException(nameof(results));

            _results = new ObservableCollection<Result>(results);
            Results = new ReadOnlyObservableCollection<Result>(_results);

            if (resultMetadatas == null) throw new ArgumentNullException(nameof(resultMetadatas));

            _resultMetadatas = new ObservableCollection<FeedResponse<dynamic>>(new List<FeedResponse<dynamic>> { resultMetadatas });
            ResultMetadata = new ResultMetrics(resultMetadatas);
        }

        public ResultSet(string error)
        {
            Error = error;            
        }

        public ResultSet(DocumentClientException documentClientException)
        {
            if (documentClientException == null) throw new ArgumentNullException(nameof(documentClientException));

            Error =
                $"Error: {documentClientException.Error}{Environment.NewLine}Message: {documentClientException.Message}{Environment.NewLine}Status Code: {documentClientException.StatusCode}{Environment.NewLine}";
        }

        public string Error { get; }

        public ReadOnlyObservableCollection<Result> Results { get; }
        public ResultMetrics ResultMetadata { get; }
        public string ResultMetadataText {  get
            {
                if (_resultMetadatas != null && _resultMetadatas.Count() > 0)
                {
                    FeedResponse<dynamic> metadata = _resultMetadatas[0];
                    ResultMetrics details = new ResultMetrics(metadata);
                    string resultMetadataText = JsonConvert.SerializeObject(details, Formatting.Indented);
                    return resultMetadataText;
                }
                return "";
            }
        }


        public void Append(IEnumerable<string> results)
        {            
            foreach (var result in results)
                _results.Add(new Result(Results.Count, result));                
        }
    }
}