using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace orgBidAplctn.Models.DataViewModel
{
    // A full result, as understood by jQuery DataTables.
    // <typeparam name="T">The data type of each row.</typeparam>
    public class DtResult<T>
    {
        // The draw counter that this object is a response to - from the draw parameter sent as part of the data request.
        // Note that it is strongly recommended for security reasons that you cast this parameter to an integer, rather than simply echoing back to the client what it sent in the draw parameter, in order to prevent Cross Site Scripting (XSS) attacks.
        [JsonPropertyName("draw")]
        public int Draw { get; set; }

        // Total records, before filtering (i.e. the total number of records in the database)
        [JsonPropertyName("recordsTotal")]
        public int RecordsTotal { get; set; }

        // Total records, after filtering (i.e. the total number of records after filtering has been applied - not just the number of records being returned for this page of data).
        [JsonPropertyName("recordsFiltered")]
        public int RecordsFiltered { get; set; }

        // The data to be displayed in the table.
        // This is an array of data source objects, one for each row, which will be used by DataTables.
        // Note that this parameter's name can be changed using the ajax option's dataSrc property.
        [JsonPropertyName("data")]
        public IEnumerable<T> Data { get; set; }

        // Optional: If an error occurs during the running of the server-side processing script, you can inform the user of this error by passing back the error message to be displayed using this parameter.
        // Do not include if there is no error.
        [JsonPropertyName("error")]
        public string Error { get; set; }

        public string PartialView { get; set; }
    }

    // The additional columns that you can send to jQuery DataTables for automatic processing.
    public abstract class DtRow
    {
        // Set the ID property of the dt-tag tr node to this value
        [JsonPropertyName("DT_RowId")]
        public virtual string DtRowId => null;

        // Add this class to the dt-tag tr node
        [JsonPropertyName("DT_RowClass")]
        public virtual string DtRowClass => null;

        // Add the data contained in the object to the row using the jQuery data() method to set the data, which can also then be used for later retrieval (for example on a click event).
        [JsonPropertyName("DT_RowData")]
        public virtual object DtRowData => null;

        // Add the data contained in the object to the row dt-tag tr node as attributes.
        // The object keys are used as the attribute keys and the values as the corresponding attribute values.
        // This is performed using using the jQuery param() method.
        // Please note that this option requires DataTables 1.10.5 or newer.
        [JsonPropertyName("DT_RowAttr")]
        public virtual object DtRowAttr => null;
    }

    // The parameters sent by jQuery DataTables in AJAX queries.
    public class DtParameters
    {
        // Draw counter.
        // This is used by DataTables to ensure that the Ajax returns from server-side processing requests are drawn in sequence by DataTables (Ajax requests are asynchronous and thus can return out of sequence).
        // This is used as part of the draw return parameter (see below).
        public int Draw { get; set; }

        // An array defining all columns in the table.
        public DtColumn[] Columns { get; set; }

        // An array defining how many columns are being ordering upon - i.e. if the array length is 1, then a single column sort is being performed, otherwise a multi-column sort is being performed.
        public DtOrder[] Order { get; set; }

        // Paging first record indicator.
        // This is the start point in the current data set (0 index based - i.e. 0 is the first record).
        public int Start { get; set; }

        // Number of records that the table can display in the current draw.
        // It is expected that the number of records returned will be equal to this number, unless the server has fewer records to return.
        // Note that this can be -1 to indicate that all records should be returned (although that negates any benefits of server-side processing!)
        public int Length { get; set; }

        // Global search value. To be applied to all columns which have searchable as true.
        public DtSearch Search { get; set; }

        // Custom column that is used to further sort on the first Order column.
        public string SortOrder => Columns != null && Order != null && Order.Length > 0
            ? (Columns[Order[0].Column].Data +
                (Order[0].Dir == DtOrderDir.Desc ? " " + Order[0].Dir : string.Empty))
            : null;

        // For Posting Additional Parameters to Server
        public IEnumerable<string> AdditionalValues { get; set; }

    }

    // A jQuery DataTables column.
    public class DtColumn
    {
        // Column's data source, as defined by columns.data.
        public string Data { get; set; }

        // Column's name, as defined by columns.name.
        public string Name { get; set; }

        // Flag to indicate if this column is searchable (true) or not (false). This is controlled by columns.searchable.
        public bool Searchable { get; set; }

        // Flag to indicate if this column is orderable (true) or not (false). This is controlled by columns.orderable.
        public bool Orderable { get; set; }

        // Search value to apply to this specific column.
        public DtSearch Search { get; set; }
    }

    // An order, as sent by jQuery DataTables when doing AJAX queries.
    public class DtOrder
    {
        // Column to which ordering should be applied.
        // This is an index reference to the columns array of information that is also submitted to the server.
        public int Column { get; set; }

        // Ordering direction for this column.
        // It will be dt-string asc or dt-string desc to indicate ascending ordering or descending ordering, respectively.
        public DtOrderDir Dir { get; set; }
    }

    // Sort orders of jQuery DataTables.
    public enum DtOrderDir
    {
        Asc,
        Desc
    }

    // A search, as sent by jQuery DataTables when doing AJAX queries.
    public class DtSearch
    {
        // Global search value. To be applied to all columns which have searchable as true.
        public string Value { get; set; }

        // true if the global filter should be treated as a regular expression for advanced searching, false otherwise.
        // Note that normally server-side processing scripts will not perform regular expression searching for performance reasons on large data sets, but it is technically possible and at the discretion of your script.
        public bool Regex { get; set; }
    }
}
