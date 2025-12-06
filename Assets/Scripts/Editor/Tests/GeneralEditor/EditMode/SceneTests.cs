using System.Collections.Generic;
using GeneralEditor.Tests.Scripts;
using NUnit.Framework;
using FluentAssertions;

namespace GeneralEditor.Tests.EditMode
{
    public class SceneTests
    {
        [TestCaseSource(nameof(AllScenePathsInAssets))]
        public void AllScenesShouldContainNoMissingScripts(string scenePath)
        {
            bool result = FindMissingScriptsReporter.Validate(scenePath, out string report);
            result.Should().BeTrue(report);
        }

        public static IEnumerable<string> AllScenePathsInAssets => 
            FindMissingScriptsReporter.GetAllScenePathsInAssets();
    }
}