namespace BookToAudio.Core.Services.Interfaces;

public interface IEmailService
{
  public  void FeedbackMessage(Dto.FeedbackRequest request);
}

