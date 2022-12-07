using System;

namespace RegistryValley.App.UserControls.TreeViewControl
{
    public class EventRegistrationToken
    {
        internal ulong m_value;

        internal EventRegistrationToken(ulong value)
        {
            m_value = value;
        }

        internal ulong Value
        {
            get { return m_value; }
        }

        public static bool operator ==(EventRegistrationToken left, EventRegistrationToken right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EventRegistrationToken left, EventRegistrationToken right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EventRegistrationToken))
            {
                return false;
            }

            return ((EventRegistrationToken)obj).Value == Value;
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }
    }
}
