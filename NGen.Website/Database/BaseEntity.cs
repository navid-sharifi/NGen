using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace NGen
{
	public abstract class BaseEntity
	{
		protected static Database Database
		{
			get
			{
				return new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<Database>();
			}
		}
		protected static GateService GateService
		{
			get
			{
				return new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<GateService>();
			}
		}
	
		public Guid Id { get; set; }

		public BaseEntity()
		{
			if (Id == Guid.Empty)
				Id = Guid.NewGuid();
		}

		public virtual Task OnSaving()
		{
			return Task.CompletedTask;
		}
	}
}
