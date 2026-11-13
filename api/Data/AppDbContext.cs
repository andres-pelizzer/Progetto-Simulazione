using System;
using System.Collections.Generic;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CategoriePermesso> CategoriePermessos { get; set; }

    public virtual DbSet<RichiestePermesso> RichiestePermessos { get; set; }

    public virtual DbSet<Utenti> Utentis { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoriePermesso>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("categorie_permesso_pkey");

            entity.ToTable("categorie_permesso");

            entity.HasIndex(e => e.Descrizione, "categorie_permesso_descrizione_key").IsUnique();

            entity.Property(e => e.CategoriaId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("categoria_id");
            entity.Property(e => e.Descrizione)
                .HasMaxLength(100)
                .HasColumnName("descrizione");
        });

        modelBuilder.Entity<RichiestePermesso>(entity =>
        {
            entity.HasKey(e => e.RichiestaId).HasName("richieste_permesso_pkey");

            entity.ToTable("richieste_permesso");

            entity.Property(e => e.RichiestaId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("richiesta_id");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.DataFine).HasColumnName("data_fine");
            entity.Property(e => e.DataInizio).HasColumnName("data_inizio");
            entity.Property(e => e.DataRichiesta)
                .HasDefaultValueSql("now()")
                .HasColumnName("data_richiesta");
            entity.Property(e => e.DataValutazione).HasColumnName("data_valutazione");
            entity.Property(e => e.Motivazione).HasColumnName("motivazione");
            entity.Property(e => e.Stato)
                .HasMaxLength(20)
                .HasDefaultValueSql("'In attesa'::character varying")
                .HasColumnName("stato");
            entity.Property(e => e.UtenteId).HasColumnName("utente_id");
            entity.Property(e => e.UtenteValutazioneId).HasColumnName("utente_valutazione_id");

            entity.HasOne(d => d.Categoria).WithMany(p => p.RichiestePermessos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("richieste_permesso_categoria_id_fkey");

            entity.HasOne(d => d.Utente).WithMany(p => p.RichiestePermessoUtentes)
                .HasForeignKey(d => d.UtenteId)
                .HasConstraintName("richieste_permesso_utente_id_fkey");

            entity.HasOne(d => d.UtenteValutazione).WithMany(p => p.RichiestePermessoUtenteValutaziones)
                .HasForeignKey(d => d.UtenteValutazioneId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("richieste_permesso_utente_valutazione_id_fkey");
        });

        modelBuilder.Entity<Utenti>(entity =>
        {
            entity.HasKey(e => e.UtenteId).HasName("utenti_pkey");

            entity.ToTable("utenti");

            entity.HasIndex(e => e.Email, "utenti_email_key").IsUnique();

            entity.Property(e => e.UtenteId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("utente_id");
            entity.Property(e => e.Cognome)
                .HasMaxLength(50)
                .HasColumnName("cognome");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("now()")
                .HasColumnName("data_creazione");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .HasColumnName("nome");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Ruolo)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Dipendente'::character varying")
                .HasColumnName("ruolo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
