using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace XmlRpc.Serialization.Tests.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestCaseAttribute<T>(params object[] arguments) : TestCaseAttribute(arguments), ITestBuilder
{
  IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test? suite)
  {
    if (method.IsGenericMethodDefinition)
    {
      method = method.MakeGenericMethod(typeof(T));
    }

    return base.BuildFrom(method, suite);
  }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestCaseAttribute<T1, T2>(params object[] arguments) : TestCaseAttribute(arguments), ITestBuilder
{
  IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test? suite)
  {
    if (method.IsGenericMethodDefinition)
    {
      method = method.MakeGenericMethod(typeof(T1), typeof(T2));
    }

    return base.BuildFrom(method, suite);
  }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestCaseAttribute<T1, T2, T3>(params object[] arguments) : TestCaseAttribute(arguments), ITestBuilder
{
  IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test? suite)
  {
    if (method.IsGenericMethodDefinition)
    {
      method = method.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3));
    }

    return base.BuildFrom(method, suite);
  }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestCaseAttribute<T1, T2, T3, T4>(params object[] arguments) : TestCaseAttribute(arguments), ITestBuilder
{
  IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test? suite)
  {
    if (method.IsGenericMethodDefinition)
    {
      method = method.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    }

    return base.BuildFrom(method, suite);
  }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestCaseAttribute<T1, T2, T3, T4, T5>(params object[] arguments) : TestCaseAttribute(arguments), ITestBuilder
{
  IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test? suite)
  {
    if (method.IsGenericMethodDefinition)
    {
      method = method.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    }

    return base.BuildFrom(method, suite);
  }
}
