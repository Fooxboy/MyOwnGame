﻿using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;

namespace MyOwnGame.Backend.Handlers;

public interface IQuestionHandler
{
    public QuestionPackType HandlerType { get; }
    
    public Task HandleSelectQuestion(Session session, Player player, Player admin, QuestionInfo question);

    public Task HandleAcceptQuestion(Session session, Player player);

    public Task HandleRejectAnswer(Session session, Player player);

    public Task HandleSetQuestionPrice(Session session, Player player, int price);
}