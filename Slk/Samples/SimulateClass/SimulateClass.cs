/* MICROSOFT PROVIDES SAMPLE CODE �AS IS� AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.� MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.� THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

// SimulateClass.cs
//
// This is SharePoint Learning Kit sample code that compiles into a console application.  You can
// compile this application using Visual Studio 2005, or you can compile and run this application
// without Visual Studio installed by double-clicking CompileAndRun.bat.
//
// This sample code is located in Samples\SLK\SimulateClass within SLK-SDK-n.n.nnn-ENU.zip.
//
// This application creates a simulated "virtual classroom" on a given SharePoint Web site.  A
// number of assignments are created, assigned to students, and set to various states.
//
// NOTE: This program can be run on its own, or it can be executed from the SimulateJobTraining
// sample application.  That sample creates a number of Web sites and executes SimulateClass on
// each one.
//
// Before running this program, the following must be true:
//
//   -- The Web site <ClassWebUrl> (below) must be a valid SharePoint Web site, accessible to the
//      current user (i.e. the person running the program).
//
//   -- That Web site must contain at least one instructor (with the SLK Instructor permission, as
//      defined in the SharePoint Learning Kit configuration page in SharePoint Central
//      Administration) and one learner (with the SLK Learner permission).
//
//   -- The documents listed in <PackageUrls> (below) must exist in SharePoint.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.SharePointLearningKit;

class SimulateClassProgram
{
    /// <summary>
    /// The SharePoint Web site (SPWeb) of the class.  This site must exist before running this
    /// program.
    /// </summary>
    const string ClassWebUrl = "http://localhost/districts/bellevue/elm/math1b";

    /// <summary>
    /// URLs of e-learning packages and/or non-e-learning documents to assign.  These files must
    /// refer to existing files within SharePoint document libraries before running this program.
    /// </summary>
    public static string[] PackageUrls = new string[]
    {
        "http://localhost/districts/library/Shared Documents/Solitaire.zip",
        "http://localhost/districts/library/Shared Documents/Pi.lrm",
        "http://localhost/districts/library/Shared Documents/Spell-Check Training.doc"
    };

    /// <summary>
    /// The number of assignments to create.  Assignments are given randomly-chosen names.
    /// </summary>
    const int NumberOfAssignments = 30;

    /// <summary>
    /// The minimum number of learners that an assignment is assigned to.  The actual number is
    /// randomly generated, between this and the number of learners specified in <c>Users</c>.
    /// Use int.MaxValue to ensure that every learner on a Web site is assigned every assignment
    /// created on that Web site.
    /// </summary>
    const int MinLearnersPerAssignment = 15;

    /// <summary>
    /// The due date of the oldest assignment, measured in "days ago".
    /// </summary>
    const double OldestAssignmentDaysAgo = 10;

    /// <summary>
    /// The due date of the newest assignment, measured in "days from now".
    /// </summary>
    const double NewestAssignmentDaysFromNow = 10;

    /// <summary>
    /// This fraction of assignments will have all learner assignments in the
    /// LearnerAssignmentState.Final state (i.e. returned to learner).
    /// </summary>
    const double FractionOfAssignmentsAllFinal = 0.15;

    /// <summary>
    /// This fraction of assignments will have all learner assignments in the
    /// LearnerAssignmentState.NotStarted state (i.e. learner hasn't begun the assignment yet).
    /// </summary>
    const double FractionOfAssignmentsNotStarted = 0.15;

    /// <summary>
    /// This fraction of "Points Possible" fields that are not already blank will be blank
    /// </summary>
    const double FractionOfBlankPointsPossible = 0.50;

    /// <summary>
    /// This fraction of "Final Points" fields will be overridden from the TotalPoints value.
    /// </summary>
    const double FractionOfOverriddenFinalPoints = 0.50;

    /// <summary>
    /// The minimum length of a randomly-generated assignment title.  Titles are generated by
    /// concatenating randomly-chosen greek letter names.
    /// </summary>
    const int MinTitleLength = 10;

    /// <summary>
    /// The maximum length of a randomly-generated assignment title.
    /// </summary>
    const int MaxTitleLength = 30;

    /// <summary>
    /// Seed for the random number generator.  Change this if you want different pseudo-random
    /// results on the next run of this program.
    /// </summary>
    const int RandomNumberSeed = 123456;

    /// <summary>
    /// Names of letters of the greek alphabet, used to construct assignment titles.
    /// </summary>
    static string[] GreekAlphabet = new string[]
    {
        "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota", "Kappa",
        "Lambda", "Mu", "Nu", "Xi", "Omicron", "Pi", "Rho", "Sigma", "Tau", "Upsilon", "Phi",
        "Chi", "Psi", "Omega"
    };

    /// <summary>
    /// Random number generator.
    /// </summary>
    static Random s_random;

    /// <summary>
    /// Informaton about a member of the class (instructor or learner).
    /// </summary>
    public class User
    {
        public string LoginName;
        public string DisplayName;

        public User(string loginName, string displayName)
        {
            LoginName = loginName;
            DisplayName = displayName;
        }
    };

    // if this program is being executed from within the ..\SimulateJobTraining sample
    // application, omit the following Main method -- SimulateJobTraining will call RunProgram
    // (below) directly
#if !OmitSimulateClassMain
    static void Main(string[] args)
    {
        RunProgram(ClassWebUrl);
    }
#endif

    public static void RunProgram(string classWebUrl)
    {
        Stack<IDisposable> disposer = new Stack<IDisposable>();
        try
        {
            // "log in" to SLK as the current user, and set <memberships> to information about
            // the instructors and learners in the class Web site (i.e. the SPWeb with URL
            // <classWebUrl>)
            SPSite spSite = new SPSite(classWebUrl);
            disposer.Push(spSite);
            SPWeb spWeb = spSite.OpenWeb();
            disposer.Push(spWeb);
            SlkStore slkStore = SlkStore.GetStore(spWeb);
            SlkMemberships memberships = slkStore.GetMemberships(spWeb, null, null);

            // make sure there's at least one instructor and one learner on the class Web; these
            // roles are defined by the "SLK Instructor" and "SLK Learner" permissions (as defined
            // in the SharePoint Learning Kit configuration page in SharePoint Central
            // Administration)
            if (memberships.Instructors.Count == 0)
                throw new Exception("Class Web must have at least one instructor");
            if (memberships.Learners.Count == 0)
                throw new Exception("Class Web must have at least one learner");

            // arbitrarily choose the first instructor in the class as the user who will create
            // the assignments
            SlkUser primaryInstructor = memberships.Instructors[0];

            // set <classWeb> to the SPWeb of the SharePoint Web site that the new assignment will
            // be associated with; "log into" this Web site as the instructor retrieved above
            SPSite classSite = new SPSite(classWebUrl, primaryInstructor.SPUser.UserToken);
            disposer.Push(classSite);
            SPWeb classWeb = classSite.OpenWeb();
            disposer.Push(classWeb);

            // set <slkStore> to the SharePoint Learning Kit store associated with the SPSite of
            // <classWeb>
            slkStore = SlkStore.GetStore(classWeb);

            // set <packageLocations> to the SharePointPackageStore-format location strings
            // corresponding to each element of <PackageUrls>; "log into" these Web sites as the
            // instructor retrieved above
            string[] packageLocations = new string[PackageUrls.Length];
            for (int packageIndex = 0; packageIndex < packageLocations.Length; packageIndex++)
            {
                // set <packageWeb> to the SPWeb of the SharePoint Web site containing the package
                // or document to assign
                string packageUrl = PackageUrls[packageIndex];
                SPSite packageSite = new SPSite(packageUrl, primaryInstructor.SPUser.UserToken);
                disposer.Push(packageSite);
                SPWeb packageWeb = packageSite.OpenWeb();
                disposer.Push(packageWeb);

                // set <spFile> to the SPFile of the package or document to assign
                SPFile spFile = packageWeb.GetFile(packageUrl);

                // set the current element of <packageLocation> to the SharePointPackageStore
                // format location string that uniquely identifies the current version of the
                // <spFile>
                packageLocations[packageIndex] = new SharePointFileLocation(
                    packageWeb, spFile.UniqueId, spFile.UIVersion).ToString();
            }

            // create a random number generator
            s_random = new Random(RandomNumberSeed);

            // set <maxNumberOfLearners> to the number of learners in the class
            int maxNumberOfLearners = memberships.Learners.Count;

            // set <learners> to an array of learners of this class; for each assignment, we'll
            // shuffle this array and choose a subset to be learners on the assignment
            SlkUser[] learners = new SlkUser[memberships.Learners.Count];
            memberships.Learners.CopyTo(learners, 0);

            // display table header
            Console.WriteLine("Assign. No. of    Due       Not");
            Console.WriteLine("ID      Learners  Date      Started Active Completed Final");
            Console.WriteLine("----------------------------------------------------------");

            // create assignments as specified by the constants at the top of this source file
            for (int assignmentIndex = 0; assignmentIndex < NumberOfAssignments; assignmentIndex++)
            {
                // set <fraction> to be proportional to <assignmentIndex>, between 0.0 and 1.0
                double fraction = (double) assignmentIndex / NumberOfAssignments;

                // randomly choose an e-learning package or non-e-learning document to be assigned
                string packageLocation = packageLocations[s_random.Next(0, PackageUrls.Length)];

                // get some information about the package/document; set <isNonELearning> to true if
                // if it's a non-e-learning document; NOTE: this is oversimplified code -- proper
                // production code should handle error conditions more carefully
                SPFile spFile = SlkUtilities.GetSPFileFromPackageLocation(packageLocation);
                bool isNonELearning;
                SharePointFileLocation spFileLocation;
                SharePointFileLocation.TryParse(packageLocation, out spFileLocation);
                using (SharePointPackageReader spPackageReader =
                    new SharePointPackageReader(slkStore.SharePointCacheSettings, spFileLocation))
                {
                    isNonELearning = PackageValidator.Validate(spPackageReader).HasErrors;
                }

                // set <assignmentProperties> to the default assignment properties for the package
                // or document being assigned; some of these properties will be overridden below
                LearningStoreXml packageWarnings;
                int? organizationIndex = (isNonELearning ? (int?) null : 0);
                AssignmentProperties assignmentProperties =
                    slkStore.GetNewAssignmentDefaultProperties(
                        classWeb, packageLocation, organizationIndex, SlkRole.Instructor,
                        out packageWarnings);

                // randomly generate a title for the assignment
                assignmentProperties.Title = CreateRandomTitle();

                // set the due date of the assignment based on <fraction>,
                // <OldestAssignmentDaysAgo>, and <NewestAssignmentDaysFromNow>
                assignmentProperties.DueDate = DateTime.Now.AddDays(
                    (OldestAssignmentDaysAgo + NewestAssignmentDaysFromNow)
                        * fraction - OldestAssignmentDaysAgo);

                // set the start date of the assignment to be a day earlier than the earliest
                // due date
                assignmentProperties.StartDate = DateTime.Today.AddDays(
                    -OldestAssignmentDaysAgo - 1);

                // randomly set Points Possible
                if ((assignmentProperties.PointsPossible == null) &&
                    (s_random.NextDouble() > FractionOfBlankPointsPossible))
                {
                    const int divideBy = 4;
                    const int maxValue = 100;
                    assignmentProperties.PointsPossible = (float) Math.Round(
                        s_random.NextDouble() * divideBy * maxValue) / maxValue;
                }

                // make all instructors of this class (i.e. all SharePoint users that have the SLK
                // Instructor permission) be instructors on this assignment
                assignmentProperties.Instructors.Clear();
                foreach (SlkUser slkUser in memberships.Instructors)
                    assignmentProperties.Instructors.Add(slkUser);

                // shuffle <learners>
                for (int learnerIndex = 0; learnerIndex < learners.Length; learnerIndex++)
                {
                    int otherLearnerIndex = s_random.Next(0, learners.Length);
                    SlkUser temp = learners[learnerIndex];
                    learners[learnerIndex] = learners[otherLearnerIndex];
                    learners[otherLearnerIndex] = temp;
                }

                // randomly choose a number of learners for this assignment
                int numberOfLearners = s_random.Next(
                    Math.Min(maxNumberOfLearners, MinLearnersPerAssignment),
                    maxNumberOfLearners + 1);

                // copy the first <numberOfLearners> learners to <assignmentProperties>
                assignmentProperties.Learners.Clear();
                for (int learnerIndex = 0; learnerIndex < numberOfLearners; learnerIndex++)
                    assignmentProperties.Learners.Add(learners[learnerIndex]);

                // create the assignment
                AssignmentItemIdentifier assignmentId = slkStore.CreateAssignment(classWeb,
                    packageLocation, organizationIndex, SlkRole.Instructor, assignmentProperties); 

                // set <gradingPropertiesList> to information about the learner assignments of the
                // new assignment; in particular, we need the learner assignment IDs
                AssignmentProperties basicAssignmentProperties;
                ReadOnlyCollection<GradingProperties> gradingPropertiesList =
                    slkStore.GetGradingProperties(assignmentId, out basicAssignmentProperties);

                // adjust the status of each learner assignment of this assignment according to
                // the rules specified in constants at the top of this source file
                int[] newStatusCount = new int[(int)LearnerAssignmentState.Final + 1];
                for (int learnerIndex = 0;
                     learnerIndex < gradingPropertiesList.Count;
                     learnerIndex++)
                {
                    // set <gradingProperties> to information about this learner assignment
                    GradingProperties gradingProperties = gradingPropertiesList[learnerIndex];

                    // set <newStatus> to the new status of the assignment, applying the rules
                    // specified in constants at the top of this source file
                    if (fraction > 1 - FractionOfAssignmentsNotStarted)
                        gradingProperties.Status = LearnerAssignmentState.NotStarted;
                    else
                    if (fraction < FractionOfAssignmentsAllFinal)
                        gradingProperties.Status = LearnerAssignmentState.Final;
                    else
                        gradingProperties.Status = (LearnerAssignmentState)
                            s_random.Next(0, (int) LearnerAssignmentState.Final + 1);

                    // if we're transitioning learner assignment to Final state, optionally
                    // assign a final points value
                    if ((gradingProperties.Status == LearnerAssignmentState.Final) &&
                        (assignmentProperties.PointsPossible != null))
                    {
                        if (s_random.NextDouble() < FractionOfOverriddenFinalPoints)
                        {
                            const int divideBy = 4;
                            gradingProperties.FinalPoints = (float) Math.Round(
                                s_random.NextDouble() * divideBy *
                                    assignmentProperties.PointsPossible.Value) / divideBy;
                        }
                    }

                    // update statistics
                    newStatusCount[(int)gradingProperties.Status]++;
                }

                // save changes to the assignment
                string warnings = slkStore.SetGradingProperties(assignmentId,
                    gradingPropertiesList);
                Debug.Assert(warnings == null, warnings);

                // display feedback
                Console.WriteLine("{0,-8}{1,-10}{2,-10:d}{3,-8}{4,-7}{5,-10}{6,-6}",
                    assignmentId.GetKey(), assignmentProperties.Learners.Count,
                    assignmentProperties.DueDate, newStatusCount[0], newStatusCount[1],
                    newStatusCount[2], newStatusCount[3]);
            }
        }
        finally
        {
            // dispose of objects used by this method
            while (disposer.Count > 0)
                disposer.Pop().Dispose();
        }
    }

    /// <summary>
    /// Returns a title consisting of randomly-chosen Greek letter names.  The title is between
    /// <c>MinTitleLength</c> and <c>MaxTitleLength</c> characters long.
    /// </summary>
    ///
    static string CreateRandomTitle()
    {
        int titleLength = s_random.Next(MinTitleLength, MaxTitleLength + 1);
        StringBuilder title = new StringBuilder(MaxTitleLength);
        while (true)
        {
            string greekLetterName = GreekAlphabet[s_random.Next(0, GreekAlphabet.Length)];
            int newLength = title.Length + 1/*space*/ + greekLetterName.Length;
            if (newLength > MaxTitleLength)
                break;
            if (title.Length > 0)
                title.Append(' ');
            title.Append(greekLetterName);
            if (newLength >= titleLength)
                break;
        }
        return title.ToString();
    }
}

