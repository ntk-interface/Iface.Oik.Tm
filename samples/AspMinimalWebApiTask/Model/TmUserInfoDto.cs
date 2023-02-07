namespace AspMinimalWebApiTask.Model;

public class TmUserInfoDto
{
  /// <summary>ID пользователя</summary>
  public int Id { get; init; }

  /// <summary>Номер группы пользователя</summary>
  public int GroupId { get; init; }

  /// <summary>Имя пользователя</summary>
  public string Name { get; init; } = "";

  /// <summary>Категория пользователя</summary>
  public string Category { get; init; } = "";
}