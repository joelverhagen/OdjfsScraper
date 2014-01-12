using System;
using System.Collections.Generic;
using System.Text;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Parser.UnitTests.Parsers.TestSupport
{
    public class ListTemplate : BaseTemplate, ITemplate<IList<ChildCareStub>>
    {
        public ListTemplate()
        {
            Model = new List<ChildCareStub>
            {
                new TypeAHomeStub
                {
                    ExternalUrlId = "AAAAAAAAAAAAAAAAAA",
                    Name = "Type A Home Name",
                    Address = "Type A Home Address",
                    City = "Type A Home City"
                },
                new TypeBHomeStub
                {
                    ExternalUrlId = "BBBBBBBBBBBBBBBBBB",
                    Name = "Type B Home Name",
                    Address = null,
                    City = "Type B Home City"
                },
                new LicensedCenterStub
                {
                    ExternalUrlId = "CCCCCCCCCCCCCCCCCC",
                    Name = "Licensed Center Name",
                    Address = "Licensed Center Address",
                    City = "Licensed Center City"
                },
                new DayCampStub
                {
                    ExternalUrlId = "DDDDDDDDDDDDDDDDDD",
                    Name = "Day Camp Name",
                    Address = "Day Camp Address",
                    City = "Day Camp City"
                }
            };
        }

        public IList<ChildCareStub> Model { get; private set; }

        public byte[] GetDocument()
        {
            var sb = new StringBuilder();

            sb.AppendLine("<table>");

            // add the header
            AddEmptyRow(sb);
            AddEmptyRow(sb);

            foreach (ChildCareStub stub in Model)
            {
                sb.AppendLine("  <tr>");
                sb.AppendLine("    <td></td><td></td>");
                sb.AppendFormat("    <td><a href='results2.asp?provider_number={0}'>{1}</a></td>", stub.ExternalUrlId, stub.Name);
                sb.AppendLine();
                sb.AppendLine("    <td></td><td></td><td></td>");
                sb.AppendFormat("    <td>{0}</td>", stub.Address);
                sb.AppendLine();
                sb.AppendLine("    <td></td><td></td><td></td>");
                sb.AppendFormat("    <td>{0}</td>", stub.City);
                sb.AppendLine();
                sb.AppendLine("    <td></td><td></td><td></td>");
                sb.AppendFormat("    <td>{0}</td>", GetStubIdentifier(stub));
                sb.AppendLine();
                sb.AppendLine("    <td></td><td></td><td></td>");
                sb.AppendFormat("    <td></td>"); // SUTQ rating, eventually
                sb.AppendLine();
                sb.AppendLine("    <td></td><td></td><td></td>");
                sb.AppendLine("    <td></td><td></td>");
                sb.AppendLine("  </tr>");

                // every data row has an empty row after it
                AddEmptyRow(sb);
            }

            sb.AppendLine("</table>");

            return GetBytes(sb.ToString());
        }

        private static void AddEmptyRow(StringBuilder sb)
        {
            sb.AppendLine("  <tr>");
            sb.AppendLine("    <td colspan='24'></td>");
            sb.AppendLine("  </tr>");
        }

        private static string GetStubIdentifier(ChildCareStub stub)
        {
            if (stub is TypeAHomeStub)
            {
                return "A";
            }
            if (stub is TypeBHomeStub)
            {
                return "B";
            }
            if (stub is LicensedCenterStub)
            {
                return "C";
            }
            if (stub is DayCampStub)
            {
                return "D";
            }
            throw new ArgumentException("The provided stub is not of an expected type.", "stub");
        }
    }
}