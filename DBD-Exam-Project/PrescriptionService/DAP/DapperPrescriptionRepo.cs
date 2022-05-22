﻿using Dapper;
using lib.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrescriptionService.DAP
{
    public class DapperPrescriptionRepo : IPrescriptionRepo
    {
        private string _connectionString;
        private string _host;
        private string _port;

        public DapperPrescriptionRepo(string connectionString, string host, string port)
        {
            _connectionString = connectionString;
            _host = host;
            _port = port;
        }

        public IEnumerable<Patient> GetAllPatients()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                DefaultTypeMap.MatchNamesWithUnderscores = true;

                var lookup = new Dictionary<string, Patient>();

                var query = @$" SELECT pat  FROM prescriptions.patient pat";



                var resultList = lookup.Values;
                return resultList;


            }

        }

        public IEnumerable<Pharmacy> GetAllPharmacies()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                DefaultTypeMap.MatchNamesWithUnderscores = true;

                var lookup = new Dictionary<string, Pharmacy>();

                var query = @$" SELECT pha  FROM prescriptions.pharmacy pha";



                var resultList = lookup.Values;
                return resultList;


            }
        }

        public IEnumerable<PrescriptionOut> GetAllPrescriptions()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                DefaultTypeMap.MatchNamesWithUnderscores = true;

                var lookup = new Dictionary<string, PrescriptionOut>();

                var query = @$" SELECT pr  FROM prescriptions.prescription pr";



                var resultList = lookup.Values;
                return resultList;


            }

        }

        

        public IEnumerable<PrescriptionOut> GetPrescriptionsExpiringLatest(DateTime expiringDate)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                var lookup = new Dictionary<long, PrescriptionOut>();

                var query = @$"
                        SELECT
                        pr.id, pr.expiration, pr.creation, pr.medicine_id, pr.prescribed_to, pr.expiration_warning_sent,
                        pat.id, pat.personal_data_id,
                        med.id, med.name,
                        dat.id, dat.email, dat.first_name, dat.last_name
                        
                        FROM prescriptions.prescription pr
                            INNER JOIN prescriptions.patient pat ON pr.prescribed_to = pat.id
                            INNER JOIN prescriptions.personal_data dat ON pat.personal_data_id = dat.id
                            INNER JOIN prescriptions.medicine med ON pr.medicine_id = med.id
                        WHERE pr.expiration < @exp::date AND pr.expiration_warning_sent != true
                        
                    ";

                var param = new DynamicParameters();
                param.Add("@exp", expiringDate.ToString("yyyy-MM-dd"));
                connection.Query<PrescriptionOut, Patient, Medicine, PersonalDatum, PrescriptionOut>(query, (pr, pat, med, dat) => {
                    PrescriptionOut prescription;
                    if (!lookup.TryGetValue(pr.Id, out prescription))
                        lookup.Add(pr.Id, prescription = pr);

                    prescription.PrescribedToNavigation = pat;
                    prescription.Medicine = med;
                    pat.PersonalData = dat;
                    return prescription;
                }, splitOn: "id, id, id, id", param: param).AsQueryable();
                var resultList = lookup.Values;



                return resultList;


            }
        }

        public IEnumerable<PrescriptionOut> GetPrescriptionsForUser(string username, string password)
        {
            Console.WriteLine($"Get prescriptions for {username.Substring(0,6)}-xxxx");

            using (var connection = new NpgsqlConnection($"Host={_host};Port={_port};Database=prescription_db; Include Error Detail=true;Username={username};Password={password}"))
            {
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                var lookup = new Dictionary<long, PrescriptionOut>();

                var query =
                    @$"
                        SELECT
                        pr.id, pr.expiration, pr.creation, pr.medicine_id, pr.prescribed_to, pr.expiration_warning_sent,
                        med.id, med.name
                        
                        FROM prescriptions.prescription pr
                            INNER JOIN prescriptions.medicine med ON pr.medicine_id = med.id
                        WHERE pr.prescribed_to_cpr like @cpr::varchar
                        
                    "
                     ;

                var param = new DynamicParameters();
                param.Add("@cpr", username);


                connection.Query<PrescriptionOut, Medicine, PrescriptionOut>(query, (pr, med) => {
                    PrescriptionOut prescription;
                    if (!lookup.TryGetValue(pr.Id, out prescription))
                        lookup.Add(pr.Id, prescription = pr);

                    prescription.Medicine = med;
                    return prescription;
                }, splitOn: "id, id", param: param).AsQueryable();
                var resultList = lookup.Values;

                return resultList;
            }
        }

        public bool MarkPrescriptionWarningSent(long prescriptionId)
        {
            Console.WriteLine($"Mark prescription with id {prescriptionId} as warned about expiration");
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                var lookup = new Dictionary<long, Prescription>();

                var query =
                    @$"
                        UPDATE prescriptions.prescription SET expiration_warning_sent = true
                        WHERE id = @id::bigint                  
                    ";

                var param = new DynamicParameters();
                param.Add("@id", prescriptionId);

                connection.Query(query);
                return true;
            }
        }
    }
}