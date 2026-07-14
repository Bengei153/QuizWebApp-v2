using System;
namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

/// <summary>
/// A folder as presented to a student — one folder = one startable quiz,
/// per the "quiz = folder" decision. Includes the parent group's name for
/// display context (e.g. "Listening — Part 1") and a live question count so
/// the UI can hide/disable folders that have no questions yet, matching
/// StartQuizHandler's own "No questions in this quiz" rule.
/// </summary>
public sealed class StudentFolderDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public Guid GroupId { get; init; }
    public string GroupName { get; init; } = "";
    public int QuestionCount { get; init; }
}
