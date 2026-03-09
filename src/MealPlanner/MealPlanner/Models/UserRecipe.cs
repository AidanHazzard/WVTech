using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Models;

[PrimaryKey(nameof(UserId), nameof(RecipeId))]
[Table("UserRecipe")]
public class UserRecipe
{
    [ForeignKey("User")]
    public string UserId { get; set; }
    
    [ForeignKey("Recipe")]
    public int RecipeId { get; set; }
    public bool UserFavorite { get; set; } = false;
    public bool UserOwner { get; set; } = false;
    public UserVoteType UserVote { get; set; } = UserVoteType.NoVote;

    public Recipe Recipe { get; set; }
    public User User { get; set; }

    public bool Redundant()
    {
        if (UserFavorite || UserOwner || UserVote != UserVoteType.NoVote)
        {
            return false;
        }
        return true;
    }
}

public enum UserVoteType
{
    NoVote,
    UpVote,
    DownVote
}