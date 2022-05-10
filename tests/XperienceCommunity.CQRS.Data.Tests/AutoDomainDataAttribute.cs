using AutoFixture.AutoNSubstitute;

// Sourced from https://tech.trailmax.info/2014/01/convert-your-projects-from-nunitmoq-to-xunit-with-nsubstitute/

namespace AutoFixture.NUnit3
{
    /// <summary>
    /// When applied to a test method, ensures that all method parameters of primitive types have random
    /// values and all non-primitive types are substitutes via <see cref="NSubstitute.Substitute"/>.
    /// </summary>
    public class AutoDomainDataAttribute : AutoDataAttribute
    {
        public AutoDomainDataAttribute()
            : base(() => new Fixture().Customize(new AutoNSubstituteCustomization()))
        {
        }
    }
}
