﻿namespace OdjfsScraper.Tool.Options
{
    public interface IOption
    {
        string Prototype { get; }
        string Description { get; }
        void Validate();
    }
}