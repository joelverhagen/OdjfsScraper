﻿using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using Ionic.Zip;
using NLog;
using OdjfsScraper.Exporter.Support;

namespace OdjfsScraper.Exporter.Exporters
{
    public class SrdsExporter<T> : ISrdsExporter<T>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ISrdsSchema<T> _schema;

        public SrdsExporter(ISrdsSchema<T> schema)
        {
            _schema = schema;
        }

        public void Export(IEnumerable<T> entities, Stream stream)
        {
            using (var zipFile = new ZipFile())
            {
                Logger.Trace("Generating AttributeKeys.csv.");
                zipFile.AddEntry("AttributeKeys.csv", GetAttributeKeyStream());

                Logger.Trace("Generating Destinations.csv.");
                zipFile.AddEntry("Destinations.csv", GetDestinationStream(entities));

                Logger.Trace("Writing the SRDS zip file to the provided stream.");
                zipFile.Save(stream);
            }
        }

        private Stream GetAttributeKeyStream()
        {
            return WriteCsv(csvWriter =>
            {
                // write the header
                csvWriter.WriteField("Name");
                csvWriter.WriteField("TypeName");
                csvWriter.NextRecord();

                // write the rows
                foreach (var srdsAttribute in _schema.GetAttributes())
                {
                    csvWriter.WriteField(srdsAttribute.Name);
                    csvWriter.WriteField(srdsAttribute.TypeName);
                    csvWriter.NextRecord();
                }
            });
        }

        private static Stream WriteCsv(Action<CsvWriter> writeAction)
        {
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            var csvWriter = new CsvWriter(streamWriter);

            writeAction(csvWriter);

            streamWriter.Flush();
            stream.Position = 0;
            return stream;
        }

        private Stream GetDestinationStream(IEnumerable<T> entities)
        {
            return WriteCsv(csvWriter =>
            {
                // write the header
                csvWriter.WriteField("Name");
                csvWriter.WriteField("Latitude");
                csvWriter.WriteField("Longitude");
                foreach (var srdsAttribute in _schema.GetAttributes())
                {
                    csvWriter.WriteField(srdsAttribute.Name);
                }
                csvWriter.NextRecord();

                // write the rows
                int i = 0;
                foreach (T entity in entities)
                {
                    csvWriter.WriteField(_schema.GetName(entity));
                    csvWriter.WriteField(_schema.GetLatitude(entity));
                    csvWriter.WriteField(_schema.GetLongitude(entity));
                    foreach (var srdsAttribute in _schema.GetAttributes())
                    {
                        csvWriter.WriteField(srdsAttribute.GetValue(entity));
                    }
                    csvWriter.NextRecord();
                    i++;
                }

                Logger.Trace("{0} destinations were written to Destinations.csv.", i);
            });
        }
    }
}