using TradingBot.Domain.Repository.PriceHistory;

namespace TradingBot.Worker.workers;
using System;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;

public class DataIngestWorker(IServiceScopeFactory scopeFactory,TimeProvider timeProvider, ILogger<SetIntervalWorker> logger) : SetIntervalWorker(timeProvider, logger)
{
    protected override string WorkerName()
    {
        return nameof(DataIngestWorker);
    }

    public override Task<bool> ShouldExecute()
    {
        return Task.FromResult(true);
    }

    public override async Task HandleExecute()
    {
        using var scope = scopeFactory.CreateScope();
        var priceHistoryRepository = scope.ServiceProvider.GetRequiredService<IPriceHistoryRepository>();
        // Specify the folder path
        string folderPath = @"D:\DataIngest";

        // Get all .csv files in the folder
        var csvFiles = Directory.EnumerateFiles(folderPath, "*.csv", SearchOption.TopDirectoryOnly);

        // Print the file names
        foreach (var file in csvFiles)
        {
            var records = ReadCsv(file);
            await priceHistoryRepository.SavePriceHistory(records);
            // delete the file after processing
            File.Delete(file);
        }
    }
    
    private IEnumerable<PriceHistoryDto> ReadCsv(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        });

        var records = new List<PriceHistoryDto>();
        if (!csv.Read() || !csv.ReadHeader()) return [];
        
        while (csv.Read())
        {
            var record = new PriceHistoryDto(
                csv.GetField<string>("name") ?? string.Empty,
                csv.GetField<string>("symbol") ?? string.Empty,
                csv.GetField<DateTimeOffset>("timeOpen").DateTime,
                csv.GetField<decimal>("open"),
                csv.GetField<DateTimeOffset>("timeHigh").DateTime,
                csv.GetField<decimal>("high"),
                csv.GetField<DateTimeOffset>("timeLow").DateTime,
                csv.GetField<decimal>("low"),
                csv.GetField<DateTimeOffset>("timeClose").DateTime,
                csv.GetField<decimal>("close"),
                csv.GetField<decimal>("volume"),
                csv.GetField<decimal>("marketCap")
            );
            records.Add(record);
        }

        return records;
    }

    public override int SleepTime()
    {
        // find next 1 minute interval and calculate time to sleep
        var now = timeProvider.GetUtcNow();
        var nextMinute = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Offset).AddMinutes(1);
        var timeToSleep = nextMinute - now;
        return (int)timeToSleep.TotalMilliseconds;
    }
}