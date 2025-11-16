namespace Factory.Domain.Entities;

public enum MachineType { Miner, Furnace, Assembler, Chemical, Lab }

public record Machine(
    string Id,
    MachineType Type,
    double SpeedMultiplier,
    double PowerUsageKw);