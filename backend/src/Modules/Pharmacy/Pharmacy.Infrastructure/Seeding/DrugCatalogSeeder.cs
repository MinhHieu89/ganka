using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Infrastructure.Seeding;

/// <summary>
/// Hosted service that seeds the drug catalog with common ophthalmic drugs.
/// Idempotent: only seeds if the DrugCatalogItems table is empty.
/// Uses DrugCatalogItem.Create() factory method for each seed item.
/// All Vietnamese names use proper diacritics per project convention.
/// </summary>
public sealed class DrugCatalogSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DrugCatalogSeeder> _logger;

    /// <summary>
    /// Default BranchId used for seed data. Matches the seeded branch in the system.
    /// </summary>
    private static readonly BranchId SeedBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public DrugCatalogSeeder(
        IServiceProvider serviceProvider,
        ILogger<DrugCatalogSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();

        try
        {
            _logger.LogInformation("DrugCatalogSeeder: Starting drug catalog seeding...");

            var existingCount = await dbContext.DrugCatalogItems.CountAsync(cancellationToken);
            if (existingCount > 0)
            {
                _logger.LogInformation(
                    "DrugCatalogSeeder: Drug catalog already seeded ({Count} items). Skipping.",
                    existingCount);
                return;
            }

            var items = GetCatalogItems();
            dbContext.DrugCatalogItems.AddRange(items);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("DrugCatalogSeeder: Seeded {Count} drug catalog items.", items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DrugCatalogSeeder: Error seeding drug catalog.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<DrugCatalogItem> GetCatalogItems()
    {
        return
        [
            // ── Antibiotics (Kháng sinh) ──────────────────────────────────────
            DrugCatalogItem.Create(
                "Tobramycin 0.3% Eye Drops", "Tobramycin 0,3% nhỏ mắt", "Tobramycin",
                DrugForm.EyeDrops, "0.3%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Ofloxacin 0.3% Eye Drops", "Ofloxacin 0,3% nhỏ mắt", "Ofloxacin",
                DrugForm.EyeDrops, "0.3%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Moxifloxacin 0.5% Eye Drops", "Moxifloxacin 0,5% nhỏ mắt", "Moxifloxacin",
                DrugForm.EyeDrops, "0.5%", DrugRoute.Topical, "Chai",
                "1 giọt x 3 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Ciprofloxacin 0.3% Eye Drops", "Ciprofloxacin 0,3% nhỏ mắt", "Ciprofloxacin",
                DrugForm.EyeDrops, "0.3%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Levofloxacin 0.5% Eye Drops", "Levofloxacin 0,5% nhỏ mắt", "Levofloxacin",
                DrugForm.EyeDrops, "0.5%", DrugRoute.Topical, "Chai",
                "1 giọt x 3-4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Chloramphenicol 0.4% Eye Drops", "Chloramphenicol 0,4% nhỏ mắt", "Chloramphenicol",
                DrugForm.EyeDrops, "0.4%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Gentamicin 0.3% Eye Drops", "Gentamicin 0,3% nhỏ mắt", "Gentamicin",
                DrugForm.EyeDrops, "0.3%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Neomycin-Polymyxin B Eye Drops", "Neomycin-Polymyxin B nhỏ mắt", "Neomycin-Polymyxin B",
                DrugForm.EyeDrops, null, DrugRoute.Topical, "Chai",
                "1-2 giọt x 4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Natamycin 5% Eye Drops", "Natamycin 5% nhỏ mắt", "Natamycin",
                DrugForm.Suspension, "5%", DrugRoute.Topical, "Chai",
                "1 giọt mỗi 1-2 giờ", SeedBranchId),

            DrugCatalogItem.Create(
                "Erythromycin 0.5% Eye Drops", "Erythromycin 0,5% nhỏ mắt", "Erythromycin",
                DrugForm.EyeDrops, "0.5%", DrugRoute.Topical, "Chai",
                "1 giọt x 4 lần/ngày", SeedBranchId),

            // ── Anti-inflammatory (Kháng viêm) ───────────────────────────────
            DrugCatalogItem.Create(
                "Dexamethasone 0.1% Eye Drops", "Dexamethasone 0,1% nhỏ mắt", "Dexamethasone",
                DrugForm.EyeDrops, "0.1%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4 lần/ngày, giảm dần liều", SeedBranchId),

            DrugCatalogItem.Create(
                "Prednisolone 1% Eye Drops", "Prednisolone 1% nhỏ mắt", "Prednisolone Acetate",
                DrugForm.Suspension, "1%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4 lần/ngày, giảm dần liều", SeedBranchId),

            DrugCatalogItem.Create(
                "Fluorometholone 0.1% Eye Drops", "Fluorometholone 0,1% nhỏ mắt", "Fluorometholone",
                DrugForm.Suspension, "0.1%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 2-4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Loteprednol 0.5% Eye Drops", "Loteprednol 0,5% nhỏ mắt", "Loteprednol Etabonate",
                DrugForm.Suspension, "0.5%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Diclofenac 0.1% Eye Drops", "Diclofenac 0,1% nhỏ mắt", "Diclofenac Sodium",
                DrugForm.EyeDrops, "0.1%", DrugRoute.Topical, "Chai",
                "1 giọt x 4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Nepafenac 0.1% Eye Drops", "Nepafenac 0,1% nhỏ mắt", "Nepafenac",
                DrugForm.Suspension, "0.1%", DrugRoute.Topical, "Chai",
                "1 giọt x 3 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Ketorolac 0.5% Eye Drops", "Ketorolac 0,5% nhỏ mắt", "Ketorolac Tromethamine",
                DrugForm.EyeDrops, "0.5%", DrugRoute.Topical, "Chai",
                "1 giọt x 4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Bromfenac 0.09% Eye Drops", "Bromfenac 0,09% nhỏ mắt", "Bromfenac Sodium",
                DrugForm.EyeDrops, "0.09%", DrugRoute.Topical, "Chai",
                "1 giọt x 2 lần/ngày", SeedBranchId),

            // ── Antiglaucoma (Hạ nhãn áp) ────────────────────────────────────
            DrugCatalogItem.Create(
                "Timolol 0.5% Eye Drops", "Timolol 0,5% nhỏ mắt", "Timolol Maleate",
                DrugForm.EyeDrops, "0.5%", DrugRoute.Topical, "Chai",
                "1 giọt x 2 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Latanoprost 0.005% Eye Drops", "Latanoprost 0,005% nhỏ mắt", "Latanoprost",
                DrugForm.EyeDrops, "0.005%", DrugRoute.Topical, "Chai",
                "1 giọt x 1 lần/ngày (tối)", SeedBranchId),

            DrugCatalogItem.Create(
                "Brimonidine 0.2% Eye Drops", "Brimonidine 0,2% nhỏ mắt", "Brimonidine Tartrate",
                DrugForm.EyeDrops, "0.2%", DrugRoute.Topical, "Chai",
                "1 giọt x 2-3 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Dorzolamide 2% Eye Drops", "Dorzolamide 2% nhỏ mắt", "Dorzolamide Hydrochloride",
                DrugForm.EyeDrops, "2%", DrugRoute.Topical, "Chai",
                "1 giọt x 3 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Travoprost 0.004% Eye Drops", "Travoprost 0,004% nhỏ mắt", "Travoprost",
                DrugForm.EyeDrops, "0.004%", DrugRoute.Topical, "Chai",
                "1 giọt x 1 lần/ngày (tối)", SeedBranchId),

            DrugCatalogItem.Create(
                "Bimatoprost 0.03% Eye Drops", "Bimatoprost 0,03% nhỏ mắt", "Bimatoprost",
                DrugForm.EyeDrops, "0.03%", DrugRoute.Topical, "Chai",
                "1 giọt x 1 lần/ngày (tối)", SeedBranchId),

            DrugCatalogItem.Create(
                "Pilocarpine 2% Eye Drops", "Pilocarpine 2% nhỏ mắt", "Pilocarpine Hydrochloride",
                DrugForm.EyeDrops, "2%", DrugRoute.Topical, "Chai",
                "1 giọt x 3-4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Acetazolamide 250mg Tablets", "Acetazolamide 250mg viên nén", "Acetazolamide",
                DrugForm.Tablet, "250mg", DrugRoute.Oral, "Viên",
                "1 viên x 2-4 lần/ngày", SeedBranchId),

            // ── Artificial Tears (Nước mắt nhân tạo) ─────────────────────────
            DrugCatalogItem.Create(
                "Hyaluronic Acid 0.1% Eye Drops", "Acid Hyaluronic 0,1% nhỏ mắt", "Sodium Hyaluronate",
                DrugForm.EyeDrops, "0.1%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4-6 lần/ngày khi cần", SeedBranchId),

            DrugCatalogItem.Create(
                "Hyaluronic Acid 0.15% Eye Drops", "Acid Hyaluronic 0,15% nhỏ mắt", "Sodium Hyaluronate",
                DrugForm.EyeDrops, "0.15%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4-6 lần/ngày khi cần", SeedBranchId),

            DrugCatalogItem.Create(
                "Hyaluronic Acid 0.3% Eye Drops", "Acid Hyaluronic 0,3% nhỏ mắt", "Sodium Hyaluronate",
                DrugForm.EyeDrops, "0.3%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 3-4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Carboxymethylcellulose 0.5% Eye Drops", "Carboxymethylcellulose 0,5% nhỏ mắt", "Carboxymethylcellulose Sodium",
                DrugForm.EyeDrops, "0.5%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4 lần/ngày khi cần", SeedBranchId),

            DrugCatalogItem.Create(
                "Polyvinyl Alcohol 1.4% Eye Drops", "Polyvinyl Alcohol 1,4% nhỏ mắt", "Polyvinyl Alcohol",
                DrugForm.EyeDrops, "1.4%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4 lần/ngày khi cần", SeedBranchId),

            DrugCatalogItem.Create(
                "Hydroxypropyl Methylcellulose 0.3% Eye Drops", "HPMC 0,3% nhỏ mắt", "Hydroxypropyl Methylcellulose",
                DrugForm.EyeDrops, "0.3%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4-6 lần/ngày khi cần", SeedBranchId),

            // ── Anti-allergy (Kháng dị ứng) ──────────────────────────────────
            DrugCatalogItem.Create(
                "Olopatadine 0.1% Eye Drops", "Olopatadine 0,1% nhỏ mắt", "Olopatadine Hydrochloride",
                DrugForm.EyeDrops, "0.1%", DrugRoute.Topical, "Chai",
                "1 giọt x 2 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Olopatadine 0.2% Eye Drops", "Olopatadine 0,2% nhỏ mắt", "Olopatadine Hydrochloride",
                DrugForm.EyeDrops, "0.2%", DrugRoute.Topical, "Chai",
                "1 giọt x 1 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Ketotifen 0.025% Eye Drops", "Ketotifen 0,025% nhỏ mắt", "Ketotifen Fumarate",
                DrugForm.EyeDrops, "0.025%", DrugRoute.Topical, "Chai",
                "1 giọt x 2 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Azelastine 0.05% Eye Drops", "Azelastine 0,05% nhỏ mắt", "Azelastine Hydrochloride",
                DrugForm.EyeDrops, "0.05%", DrugRoute.Topical, "Chai",
                "1 giọt x 2 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Cromolyn Sodium 4% Eye Drops", "Cromolyn Natri 4% nhỏ mắt", "Cromolyn Sodium",
                DrugForm.EyeDrops, "4%", DrugRoute.Topical, "Chai",
                "1-2 giọt x 4-6 lần/ngày", SeedBranchId),

            // ── Mydriatics / Cycloplegics (Giãn đồng tử / Liệt điều tiết) ───
            DrugCatalogItem.Create(
                "Tropicamide 1% Eye Drops", "Tropicamide 1% nhỏ mắt", "Tropicamide",
                DrugForm.EyeDrops, "1%", DrugRoute.Topical, "Chai",
                "1-2 giọt, chờ 15-20 phút", SeedBranchId),

            DrugCatalogItem.Create(
                "Tropicamide 0.5% Eye Drops", "Tropicamide 0,5% nhỏ mắt", "Tropicamide",
                DrugForm.EyeDrops, "0.5%", DrugRoute.Topical, "Chai",
                "1-2 giọt, chờ 15-20 phút", SeedBranchId),

            DrugCatalogItem.Create(
                "Atropine 1% Eye Drops", "Atropine 1% nhỏ mắt", "Atropine Sulfate",
                DrugForm.EyeDrops, "1%", DrugRoute.Topical, "Chai",
                "1 giọt x 1-2 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Atropine 0.01% Eye Drops", "Atropine 0,01% nhỏ mắt", "Atropine Sulfate",
                DrugForm.EyeDrops, "0.01%", DrugRoute.Topical, "Chai",
                "1 giọt x 1 lần/ngày (tối)", SeedBranchId),

            DrugCatalogItem.Create(
                "Phenylephrine 2.5% Eye Drops", "Phenylephrine 2,5% nhỏ mắt", "Phenylephrine Hydrochloride",
                DrugForm.EyeDrops, "2.5%", DrugRoute.Topical, "Chai",
                "1 giọt, chờ 15-20 phút", SeedBranchId),

            DrugCatalogItem.Create(
                "Cyclopentolate 1% Eye Drops", "Cyclopentolate 1% nhỏ mắt", "Cyclopentolate Hydrochloride",
                DrugForm.EyeDrops, "1%", DrugRoute.Topical, "Chai",
                "1 giọt x 1-2 lần, chờ 30-60 phút", SeedBranchId),

            // ── Combination (Kết hợp) ────────────────────────────────────────
            DrugCatalogItem.Create(
                "Tobramycin-Dexamethasone Eye Drops", "Tobramycin-Dexamethasone nhỏ mắt", "Tobramycin/Dexamethasone",
                DrugForm.Suspension, null, DrugRoute.Topical, "Chai",
                "1-2 giọt x 4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Neomycin-Dexamethasone Eye Drops", "Neomycin-Dexamethasone nhỏ mắt", "Neomycin/Dexamethasone",
                DrugForm.EyeDrops, null, DrugRoute.Topical, "Chai",
                "1-2 giọt x 3-4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Ofloxacin-Dexamethasone Eye Drops", "Ofloxacin-Dexamethasone nhỏ mắt", "Ofloxacin/Dexamethasone",
                DrugForm.EyeDrops, null, DrugRoute.Topical, "Chai",
                "1-2 giọt x 3-4 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Dorzolamide-Timolol Eye Drops", "Dorzolamide-Timolol nhỏ mắt", "Dorzolamide/Timolol",
                DrugForm.EyeDrops, "2%/0.5%", DrugRoute.Topical, "Chai",
                "1 giọt x 2 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Brimonidine-Timolol Eye Drops", "Brimonidine-Timolol nhỏ mắt", "Brimonidine/Timolol",
                DrugForm.EyeDrops, "0.2%/0.5%", DrugRoute.Topical, "Chai",
                "1 giọt x 2 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Latanoprost-Timolol Eye Drops", "Latanoprost-Timolol nhỏ mắt", "Latanoprost/Timolol",
                DrugForm.EyeDrops, "0.005%/0.5%", DrugRoute.Topical, "Chai",
                "1 giọt x 1 lần/ngày (sáng)", SeedBranchId),

            // ── Oral medications (Thuốc uống) ────────────────────────────────
            DrugCatalogItem.Create(
                "Ciprofloxacin 500mg Tablets", "Ciprofloxacin 500mg viên nén", "Ciprofloxacin",
                DrugForm.Tablet, "500mg", DrugRoute.Oral, "Viên",
                "1 viên x 2 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Doxycycline 100mg Capsules", "Doxycycline 100mg viên nang", "Doxycycline",
                DrugForm.Capsule, "100mg", DrugRoute.Oral, "Viên",
                "1 viên x 2 lần/ngày (sau ăn)", SeedBranchId),

            DrugCatalogItem.Create(
                "Amoxicillin 500mg Capsules", "Amoxicillin 500mg viên nang", "Amoxicillin",
                DrugForm.Capsule, "500mg", DrugRoute.Oral, "Viên",
                "1 viên x 3 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Prednisolone 5mg Tablets", "Prednisolone 5mg viên nén", "Prednisolone",
                DrugForm.Tablet, "5mg", DrugRoute.Oral, "Viên",
                "Theo chỉ định bác sĩ, giảm dần liều", SeedBranchId),

            DrugCatalogItem.Create(
                "Methylprednisolone 4mg Tablets", "Methylprednisolone 4mg viên nén", "Methylprednisolone",
                DrugForm.Tablet, "4mg", DrugRoute.Oral, "Viên",
                "Theo chỉ định bác sĩ, giảm dần liều", SeedBranchId),

            DrugCatalogItem.Create(
                "Omega-3 Supplement", "Omega-3 thực phẩm chức năng", "Omega-3 Fatty Acids",
                DrugForm.Capsule, null, DrugRoute.Oral, "Viên",
                "1-2 viên/ngày sau ăn", SeedBranchId),

            DrugCatalogItem.Create(
                "Vitamin A 25000IU Capsules", "Vitamin A 25000IU viên nang", "Retinol",
                DrugForm.Capsule, "25000IU", DrugRoute.Oral, "Viên",
                "1 viên/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Cetirizine 10mg Tablets", "Cetirizine 10mg viên nén", "Cetirizine Hydrochloride",
                DrugForm.Tablet, "10mg", DrugRoute.Oral, "Viên",
                "1 viên/ngày (tối)", SeedBranchId),

            DrugCatalogItem.Create(
                "Ibuprofen 400mg Tablets", "Ibuprofen 400mg viên nén", "Ibuprofen",
                DrugForm.Tablet, "400mg", DrugRoute.Oral, "Viên",
                "1 viên x 2-3 lần/ngày (sau ăn)", SeedBranchId),

            DrugCatalogItem.Create(
                "Paracetamol 500mg Tablets", "Paracetamol 500mg viên nén", "Paracetamol",
                DrugForm.Tablet, "500mg", DrugRoute.Oral, "Viên",
                "1-2 viên x 3-4 lần/ngày (cách 4-6 giờ)", SeedBranchId),

            // ── Ointments (Thuốc mỡ) ─────────────────────────────────────────
            DrugCatalogItem.Create(
                "Erythromycin 0.5% Eye Ointment", "Erythromycin 0,5% mỡ tra mắt", "Erythromycin",
                DrugForm.Ointment, "0.5%", DrugRoute.Topical, "Tuýp",
                "Tra 1 lần/ngày (trước khi ngủ)", SeedBranchId),

            DrugCatalogItem.Create(
                "Tetracycline 1% Eye Ointment", "Tetracycline 1% mỡ tra mắt", "Tetracycline",
                DrugForm.Ointment, "1%", DrugRoute.Topical, "Tuýp",
                "Tra 2-3 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Acyclovir 3% Eye Ointment", "Acyclovir 3% mỡ tra mắt", "Acyclovir",
                DrugForm.Ointment, "3%", DrugRoute.Topical, "Tuýp",
                "Tra 5 lần/ngày (cách 4 giờ)", SeedBranchId),

            DrugCatalogItem.Create(
                "Tobramycin-Dexamethasone Eye Ointment", "Tobramycin-Dexamethasone mỡ tra mắt", "Tobramycin/Dexamethasone",
                DrugForm.Ointment, null, DrugRoute.Topical, "Tuýp",
                "Tra 2-3 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Dexamethasone Eye Ointment", "Dexamethasone mỡ tra mắt", "Dexamethasone",
                DrugForm.Ointment, "0.1%", DrugRoute.Topical, "Tuýp",
                "Tra 1-2 lần/ngày", SeedBranchId),

            // ── Antiviral (Kháng virus) ───────────────────────────────────────
            DrugCatalogItem.Create(
                "Ganciclovir 0.15% Eye Gel", "Ganciclovir 0,15% gel tra mắt", "Ganciclovir",
                DrugForm.Gel, "0.15%", DrugRoute.Topical, "Tuýp",
                "1 giọt x 5 lần/ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Acyclovir 400mg Tablets", "Acyclovir 400mg viên nén", "Acyclovir",
                DrugForm.Tablet, "400mg", DrugRoute.Oral, "Viên",
                "1 viên x 5 lần/ngày x 7-10 ngày", SeedBranchId),

            DrugCatalogItem.Create(
                "Valacyclovir 500mg Tablets", "Valacyclovir 500mg viên nén", "Valacyclovir",
                DrugForm.Tablet, "500mg", DrugRoute.Oral, "Viên",
                "1 viên x 3 lần/ngày x 7 ngày", SeedBranchId),

            // ── Diagnostic agents (Thuốc chẩn đoán) ──────────────────────────
            DrugCatalogItem.Create(
                "Fluorescein Sodium 0.25% Eye Drops", "Fluorescein Natri 0,25% nhỏ mắt", "Fluorescein Sodium",
                DrugForm.EyeDrops, "0.25%", DrugRoute.Topical, "Chai",
                "1 giọt (chẩn đoán)", SeedBranchId),

            DrugCatalogItem.Create(
                "Proparacaine 0.5% Eye Drops", "Proparacaine 0,5% nhỏ mắt", "Proparacaine Hydrochloride",
                DrugForm.EyeDrops, "0.5%", DrugRoute.Topical, "Chai",
                "1-2 giọt (gây tê bề mặt)", SeedBranchId),

            DrugCatalogItem.Create(
                "Tetracaine 0.5% Eye Drops", "Tetracaine 0,5% nhỏ mắt", "Tetracaine Hydrochloride",
                DrugForm.EyeDrops, "0.5%", DrugRoute.Topical, "Chai",
                "1-2 giọt (gây tê bề mặt)", SeedBranchId),

            // ── Injection (Thuốc tiêm) ───────────────────────────────────────
            DrugCatalogItem.Create(
                "Bevacizumab 25mg/ml Injection", "Bevacizumab 25mg/ml tiêm", "Bevacizumab",
                DrugForm.Injection, "25mg/ml", DrugRoute.Intravitreal, "Lọ",
                "1.25mg/0.05ml tiêm nội nhãn", SeedBranchId),

            DrugCatalogItem.Create(
                "Ranibizumab 10mg/ml Injection", "Ranibizumab 10mg/ml tiêm", "Ranibizumab",
                DrugForm.Injection, "10mg/ml", DrugRoute.Intravitreal, "Lọ",
                "0.5mg/0.05ml tiêm nội nhãn", SeedBranchId),

            DrugCatalogItem.Create(
                "Triamcinolone 40mg/ml Injection", "Triamcinolone 40mg/ml tiêm", "Triamcinolone Acetonide",
                DrugForm.Injection, "40mg/ml", DrugRoute.Subconjunctival, "Lọ",
                "Theo chỉ định bác sĩ", SeedBranchId),

            DrugCatalogItem.Create(
                "Dexamethasone 4mg/ml Injection", "Dexamethasone 4mg/ml tiêm", "Dexamethasone Sodium Phosphate",
                DrugForm.Injection, "4mg/ml", DrugRoute.Subconjunctival, "Ống",
                "Theo chỉ định bác sĩ", SeedBranchId),

            // ── Dry Eye / Specialty (Khô mắt / Chuyên biệt) ─────────────────
            DrugCatalogItem.Create(
                "Cyclosporine 0.05% Eye Drops", "Cyclosporine 0,05% nhỏ mắt", "Cyclosporine",
                DrugForm.EyeDrops, "0.05%", DrugRoute.Topical, "Lọ",
                "1 giọt x 2 lần/ngày (cách 12 giờ)", SeedBranchId),

            DrugCatalogItem.Create(
                "Lifitegrast 5% Eye Drops", "Lifitegrast 5% nhỏ mắt", "Lifitegrast",
                DrugForm.EyeDrops, "5%", DrugRoute.Topical, "Lọ",
                "1 giọt x 2 lần/ngày (cách 12 giờ)", SeedBranchId),

            DrugCatalogItem.Create(
                "Carbomer Gel Eye Drops", "Carbomer gel nhỏ mắt", "Carbomer",
                DrugForm.Gel, "0.2%", DrugRoute.Topical, "Tuýp",
                "1 giọt x 3-4 lần/ngày", SeedBranchId),
        ];
    }
}
