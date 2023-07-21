﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Shiny.Support.Repositories;

namespace Shiny.Net.Http;


public record HttpTransferRequest(
    string Identifier,
    string Uri,
    bool IsUpload,
    string LocalFilePath,
    bool UseMeteredConnection = true,
    TransferHttpContent? HttpContent = null,
    string? HttpMethod = null,
    IDictionary<string, string>? Headers = null
)
{
    
    public HttpMethod GetHttpMethod()
    {
        var defMethod = this.IsUpload ? "POST" : "GET";
        var httpMethod = new HttpMethod(this.HttpMethod ?? defMethod);

        return httpMethod;
    }
}


public record HttpTransfer(
    HttpTransferRequest Request,
    long? BytesToTransfer,
    long BytesTransferred,
    HttpTransferState Status,
    DateTimeOffset CreatedAt
) : IRepositoryEntity
{
    public string Identifier => this.Request.Identifier;
    public bool IsDeterministic => this.BytesToTransfer != null;
};

public record HttpTransferResult(
    HttpTransferRequest Request,
    HttpTransferState Status,
    TransferProgress Progress,
    Exception? Exception
)
{
    public bool IsDeterministic => this.Progress.BytesToTransfer != null;
};


public record TransferProgress(    
    long BytesPerSecond,
    long? BytesToTransfer,
    long BytesTransferred
)
{
    public bool IsDeterministic => this.BytesToTransfer != null;


    double? percentComplete;
    public double PercentComplete
    {
        get
        {
            if (this.percentComplete == null)
            {
                if (this.BytesToTransfer == null)
                {
                    this.percentComplete = -1;
                }
                else
                {
                    this.percentComplete = Math.Round((double)this.BytesTransferred / this.BytesToTransfer!.Value, 2);
                }
            }
            return this.percentComplete.Value;
        }
    }


    TimeSpan? estimate;
    public TimeSpan EstimatedTimeRemaining
    {
        get
        {
            if (this.estimate == null)
            {
                if (this.BytesToTransfer == null || this.BytesPerSecond == 0)
                {
                    this.estimate = TimeSpan.Zero;
                }
                else
                {
                    var bytesRemaining = this.BytesToTransfer.Value - this.BytesTransferred;
                    this.estimate = TimeSpan.FromSeconds(bytesRemaining / this.BytesPerSecond);
                }
            }
            return this.estimate!.Value;
        }
    }
};


public record TransferHttpContent(
    string Content,
    string ContentType = "text/plain",
    string Encoding = "utf-8"
)
{
    public static TransferHttpContent FromJson(object obj, JsonSerializerOptions? jsonOptions = null)
    {
        var json = JsonSerializer.Serialize(obj, jsonOptions);
        return new TransferHttpContent(
            json,
            "application/json",
            "utf-8"
        );
    }
}


public enum HttpTransferState
{
    Unknown,
    Pending,
    Paused,
    PausedByNoNetwork,
    PausedByCostedNetwork,
    InProgress,
    Error,
    Canceled,
    Completed
}