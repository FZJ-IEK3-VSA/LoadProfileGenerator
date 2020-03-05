using System;

namespace Database.Tables {
    [AttributeUsage(AttributeTargets.Property)]

    public class IgnoreForJsonSyncAttribute : Attribute
    {
    }
}