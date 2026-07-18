using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using System;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public record SubmitQuizCommand(Guid AttemptId, Dictionary<Guid, List<Guid>> Answers)
    : IRequest<QuizResultDto>;