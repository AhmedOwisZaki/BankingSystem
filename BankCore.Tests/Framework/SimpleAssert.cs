using System;

namespace BankCore.Tests.Framework
{
    public static class SimpleAssert
    {
        public static void AreEqual<T>(T expected, T actual, string message = "")
        {
            if (!object.Equals(expected, actual))
            {
                throw new Exception($"Assert.AreEqual failed. Expected: <{expected}>. Actual: <{actual}>. {message}");
            }
        }

        public static void AreNotEqual<T>(T notExpected, T actual, string message = "")
        {
            if (object.Equals(notExpected, actual))
            {
                throw new Exception($"Assert.AreNotEqual failed. Did not expect: <{notExpected}>. Actual: <{actual}>. {message}");
            }
        }

        public static void IsTrue(bool condition, string message = "")
        {
            if (!condition)
            {
                throw new Exception($"Assert.IsTrue failed. {message}");
            }
        }

        public static void IsFalse(bool condition, string message = "")
        {
            if (condition)
            {
                throw new Exception($"Assert.IsFalse failed. {message}");
            }
        }

        public static void IsNull(object obj, string message = "")
        {
            if (obj != null)
            {
                throw new Exception($"Assert.IsNull failed. Expected null but got an object. {message}");
            }
        }

        public static void IsNotNull(object obj, string message = "")
        {
            if (obj == null)
            {
                throw new Exception($"Assert.IsNotNull failed. Expected non-null object. {message}");
            }
        }

        public static void Throws<TException>(Action action, string message = "") where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return; // Passed
            }
            catch (Exception ex)
            {
                throw new Exception($"Assert.Throws failed. Expected exception of type {typeof(TException).Name}, but got exception of type {ex.GetType().Name}. {message}");
            }
            throw new Exception($"Assert.Throws failed. Expected exception of type {typeof(TException).Name}, but no exception was thrown. {message}");
        }
    }
}
