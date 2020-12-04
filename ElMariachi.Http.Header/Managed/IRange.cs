using System;

namespace ElMariachi.Http.Header.Managed
{
    /// <summary>
    /// Represents a part of a requested document
    /// </summary>
    public interface IRange: IEquatable<IRange>
    {
        /// <summary>
        /// Event triggered when the range is changed
        /// </summary>
        public event RangeChangedHandler Changed;

        /// <summary>
        /// Get or set the first part index of the requested document.
        /// When this property is null, the part of the requested document corresponds to the number of bytes specified by <see cref="End"/> property, starting from the end of the requested document
        /// </summary>
        public long? Start { get; set; }

        /// <summary>
        /// Get or set the last part index of the requested document (only when <see cref="Start"/> is not null).
        /// When <see cref="Start"/> is null, this property corresponds to the number of bytes requested starting from the end of the requested document
        /// When this property is null, the requested part of the document starts from the specified <see cref="Start"/> index until the end of the requested document.
        /// </summary>
        public long? End { get; set; }

    }

    public delegate void RangeChangedHandler(object sender, RangeChangedHandlerArgs args);

    public class RangeChangedHandlerArgs
    {
        public RangeChangedHandlerArgs(string propertyName)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        }

        public string PropertyName { get; }
    }
}