using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TVPlayer.CRUD.Classes;
using TVPlayer.CRUD.Interfaces.Repositories;
using TVPlayer.CRUD.Models;

namespace TVPlayer.CRUD.Repositories
{
    public class ProgramacaoRepository : IProgramacaoRepository
    {
        private const string PROCEDURE_PROGRAMACOES_DO_USUARIO = "ap_GetProgramacaoByUsuarioID";

        private VideoContext _context;

        public ProgramacaoRepository(VideoContext context) => _context = context;

        public async Task<IEnumerable<ProgramacaoResumo>> GetProgramacoesByUsuarioIDAsync(int usuarioID)
        {
            var programacoes = new List<ProgramacaoResumo>();
            var connection = _context.Database.GetDbConnection();
            var abertaAqui = connection.State != ConnectionState.Open;

            if (abertaAqui)
                await connection.OpenAsync();

            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = PROCEDURE_PROGRAMACOES_DO_USUARIO;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

                    var parametro = command.CreateParameter();
                    parametro.ParameterName = "@UsuarioID";
                    parametro.DbType = DbType.Int32;
                    parametro.Value = usuarioID;
                    command.Parameters.Add(parametro);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            programacoes.Add(new ProgramacaoResumo(
                                reader.GetInt32(reader.GetOrdinal("ID")),
                                reader.GetString(reader.GetOrdinal("Nome")),
                                reader.GetString(reader.GetOrdinal("Categoria"))));
                        }
                    }
                }
            }
            finally
            {
                if (abertaAqui)
                    connection.Close();
            }

            return programacoes;
        }

        public async Task AddAsync(Programacao entity)
        {
            try { await _context.Programacao.AddAsync(entity); }
            catch (Exception ex) { throw ex; }
        }
    }
}