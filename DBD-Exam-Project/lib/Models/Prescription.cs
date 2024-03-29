﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using MessagePack;

namespace lib.Models;

[Table("prescription", Schema = "prescriptions"), MessagePackObject(keyAsPropertyName: true)]
public class Prescription: EntityWithId<long>
{
    [Column("id"), System.ComponentModel.DataAnnotations.Key, MessagePack.Key(nameof(Id))]
    public long Id { get; set; }
    [Column("expiration")]
    public DateTime? Expiration { get; set; }
    [Column("expiration_warning_sent")]
    public bool ExpirationWarningSent { get; set; }
    [Column("creation", TypeName = "timestamp without time zone")]
    public DateTime Creation { get; set; }
    [Column("medicine_id")]
    public int MedicineId { get; set; }
    [Column("prescribed_by")]
    public int PrescribedBy { get; set; }
    [Column("prescribed_to")]
    public int PrescribedTo { get; set; }
    [Column("prescribed_to_cpr")]
    public string PrescribedToCpr { get; set; } = null!;
    [Column("last_administered_by")]
    public int? LastAdministeredBy { get; set; }

    public Pharmaceut? LastAdministeredByNavigation { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public Doctor? PrescribedByNavigation { get; set; }
    public Patient? PrescribedToNavigation { get; set; }
}