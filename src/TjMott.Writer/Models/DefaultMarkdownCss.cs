using System;
using System.Collections.Generic;
using System.Text;

namespace TjMott.Writer.Models
{
    public static class DefaultMarkdownCss
    {
        public static readonly string DefaultCss = @"
<style>
table {
  border-collapse:collapse;
}
table, th, td {
  border: 1px solid black;
}
th, td {
  padding: 10px;
}
th {
  background-color: darkblue;
  color: #F9F9F9;
}
td {
  background-color: #F9F9F9;
  color: darkblue;
}
</style>
";
    }
}
