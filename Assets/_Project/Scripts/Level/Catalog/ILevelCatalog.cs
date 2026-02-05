using System.Collections.Generic;

public interface ILevelCatalog
{
    IReadOnlyList<LevelCatalogEntry> GetAll();
}
