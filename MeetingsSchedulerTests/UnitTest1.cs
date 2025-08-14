using System.Collections;
using MeetingScheduler.Services;
using MeetingScheduler.DTOs;
using MeetingScheduler.Models;
using System.Reflection;

namespace MeetingScheduler.Tests
{
    public class MeetingsServiceTests
    {
        public MeetingsServiceTests()
        {
            ClearStaticData();
        }
        
        private static void ClearStaticData()
        {
            var serviceType = typeof(MeetingsService);
            
            var meetingsField = serviceType
                .GetField("_meetings", BindingFlags.NonPublic | BindingFlags.Static);
            var usersField = serviceType
                .GetField("_users", BindingFlags.NonPublic | BindingFlags.Static);
            var meetingCountField = serviceType
                .GetField("_meetingCount", BindingFlags.NonPublic | BindingFlags.Static);
            
            if (meetingsField?.GetValue(null) is IList meetingsList)
                meetingsList.Clear();
                
            if (usersField?.GetValue(null) is IList usersList)
                usersList.Clear();
                
            meetingCountField?.SetValue(null, 0);
        }
        [Fact]
        public async Task CreateMeeting_WithNoExistingMeetings_ReturnsEarliestStartTime()
        {
            var meetingsService = new MeetingsService();
            var usersService = new UsersService();
            await usersService.CreateUser(new UserCreateRequestModel { Id = 1, Name = "User A" });
            await usersService.CreateUser(new UserCreateRequestModel { Id = 2, Name = "User B" });
            
            var request = new MeetingCreateRequestModel
            {
                ParticipantIds = new List<int> { 1, 2 },
                DurationMinutes = 60,
                EarliestStart = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc),
                LatestEnd = new DateTime(2025, 6, 20, 17, 0, 0, DateTimeKind.Utc)
            };

            var meeting = await meetingsService.CreateMeeting(request);

            Assert.NotNull(meeting);
            Assert.Equal(request.EarliestStart, meeting.StartTime);
            Assert.Equal(request.EarliestStart.AddMinutes(60), meeting.EndTime);
            Assert.Equal(2, meeting.Participants.Count);
        }

        [Fact]
        public async Task CreateMeeting_WithConflictingMeeting_SchedulesAfterConflict()
        {
            var meetingsService = new MeetingsService();
            var usersService = new UsersService();
            await usersService.CreateUser(new UserCreateRequestModel { Id = 1, Name = "User A" });
            
            var earliestStart = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc);
            var latestEnd = new DateTime(2025, 6, 20, 17, 0, 0, DateTimeKind.Utc);
            
            var firstRequest = new MeetingCreateRequestModel
            {
                ParticipantIds = new List<int> { 1 },
                DurationMinutes = 60,
                EarliestStart = earliestStart,
                LatestEnd = latestEnd
            };
            await meetingsService.CreateMeeting(firstRequest);
            
            var secondRequest = new MeetingCreateRequestModel
            {
                ParticipantIds = new List<int> { 1 },
                DurationMinutes = 30,
                EarliestStart = earliestStart,
                LatestEnd = latestEnd
            };

            var meeting = await meetingsService.CreateMeeting(secondRequest);

            Assert.NotNull(meeting);
            Assert.Equal(earliestStart.AddMinutes(60), meeting.StartTime); // Should start at 10:00
        }

        [Fact]
        public async Task CreateMeeting_WithOverlapAllowed_SchedulesWithOverlap()
        {
            var usersService = new UsersService();
            var meetingsService = new MeetingsService();
            await usersService.CreateUser(new UserCreateRequestModel { Id = 1, Name = "User A" });
            
            var earliestStart = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc);
            var latestEnd = new DateTime(2025, 6, 20, 10, 30, 0, DateTimeKind.Utc);
            
            var firstRequest = new MeetingCreateRequestModel
            {
                ParticipantIds = new List<int> { 1 },
                DurationMinutes = 60,
                EarliestStart = earliestStart,
                LatestEnd = latestEnd
            };
            await meetingsService.CreateMeeting(firstRequest);

            var secondRequest = new MeetingCreateRequestModel
            {
                ParticipantIds = new List<int> { 1 },
                DurationMinutes = 45,
                EarliestStart = earliestStart,
                LatestEnd = latestEnd
            };

            var meeting = await meetingsService.CreateMeeting(secondRequest);

            Assert.NotNull(meeting);
            Assert.Equal(earliestStart.AddMinutes(45), meeting.StartTime);
        }

        [Fact]
        public async Task CreateMeeting_NoAvailableSlot_ReturnsNull()
        {
            var usersService = new UsersService();
            var meetingsService = new MeetingsService();
            await usersService.CreateUser(new UserCreateRequestModel { Id = 1, Name = "User A" });
            
            var earliestStart = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc);
            var latestEnd = new DateTime(2025, 6, 20, 10, 0, 0, DateTimeKind.Utc);
            
            var firstRequest = new MeetingCreateRequestModel
            {
                ParticipantIds = new List<int> { 1 },
                DurationMinutes = 60,
                EarliestStart = earliestStart,
                LatestEnd = latestEnd
            };
            await meetingsService.CreateMeeting(firstRequest);

            var secondRequest = new MeetingCreateRequestModel
            {
                ParticipantIds = new List<int> { 1 },
                DurationMinutes = 60,
                EarliestStart = earliestStart,
                LatestEnd = latestEnd
            };

            var meeting = await meetingsService.CreateMeeting(secondRequest);

            Assert.Null(meeting);
        }

        [Fact]
        public async Task GetAllMeetingsForUser_ReturnsOnlyUserMeetings()
        {
            var meetingsService = new MeetingsService();
            var usersService = new UsersService();
            await usersService.CreateUser(new UserCreateRequestModel { Id = 1, Name = "User A" });
            await usersService.CreateUser(new UserCreateRequestModel { Id = 2, Name = "User B" });
            
            var startTime = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc);
            var endTime = new DateTime(2025, 6, 20, 17, 0, 0, DateTimeKind.Utc);
            
            await meetingsService.CreateMeeting(new MeetingCreateRequestModel
            {
                ParticipantIds = new List<int> { 1 },
                DurationMinutes = 60,
                EarliestStart = startTime,
                LatestEnd = endTime
            });
            
            await meetingsService.CreateMeeting(new MeetingCreateRequestModel
            {
                ParticipantIds = new List<int> { 2 },
                DurationMinutes = 60,
                EarliestStart = startTime.AddHours(1),
                LatestEnd = endTime
            });
            
            await meetingsService.CreateMeeting(new MeetingCreateRequestModel
            {
                ParticipantIds = new List<int> { 1, 2 },
                DurationMinutes = 60,
                EarliestStart = startTime.AddHours(2),
                LatestEnd = endTime
            });

            var user1Meetings = await meetingsService.GetAllMeetingsForUser(1);
            var user2Meetings = await meetingsService.GetAllMeetingsForUser(2);

            Assert.Equal(2, user1Meetings.Count);
            Assert.Equal(2, user2Meetings.Count);
        }

        [Fact]
        public async Task CreateUser_StoresUserCorrectly()
        {
            var usersService = new UsersService();
            var userRequest = new UserCreateRequestModel { Id = 123, Name = "Test User" };

            await usersService.CreateUser(userRequest);
            var retrievedUser = await usersService.GetUserById(123);

            Assert.NotNull(retrievedUser);
            Assert.Equal(123, retrievedUser.Id);
            Assert.Equal("Test User", retrievedUser.Name);
        }
    }
}