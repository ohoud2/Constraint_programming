using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConstraintProgramming.Entities {
    public class Store {
        public Store() {
            Products = new HashSet<Product>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public double LocationX { get; set; }

        [Required]
        public double LocationY { get; set; }

        public virtual ICollection<Product> Products { get; set; }

        public double getDistance(double userX, double userY) {
            return 0;
        }
    }
}