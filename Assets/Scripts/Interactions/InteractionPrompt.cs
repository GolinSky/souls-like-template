using System;

namespace SoulsLike.Interactions
{
    public readonly struct InteractionPrompt : IEquatable<InteractionPrompt>
    {
        public string Text { get; }
        public bool IsVisible => !string.IsNullOrEmpty(Text);

        public InteractionPrompt(string text)
        {
            Text = text;
        }

        public bool Equals(InteractionPrompt other) => Text == other.Text;

        public override bool Equals(object obj) =>
            obj is InteractionPrompt other && Equals(other);

        public override int GetHashCode() => Text?.GetHashCode() ?? 0;
    }
}
