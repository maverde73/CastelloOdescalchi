using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;

namespace Presentation
{
    public class CustomScheduleViewDragDropBehavior : ScheduleViewDragDropBehavior
    {
        public override bool CanDrop(DragDropState state)
        {
            return false;
        }

        public override void Drop(DragDropState state)
        {
            //var appointment = state.Appointment as CustomAppointment;

            //if (appointment.IsDraggedFromListBox)
            //{
            //    appointment.Body = "Dragged from the ListBox";
            //}

            //if (state.IsControlPressed)
            //{
            //    state.IsControlPressed = false;
            //}

            base.Drop(state);

        }

        public override bool CanResize(DragDropState state)
        {
            //var destinationSlot = state.DestinationSlots.First() as Slot;
            //var duration = destinationSlot.End - destinationSlot.Start;

            //if (duration <= new TimeSpan(0, 30, 0) || duration >= new TimeSpan(2, 0, 1))
            //{
            //    return false;
            //}

            //return base.CanResize(state);

            return false;
        }

        public override bool CanStartResize(DragDropState state)
        {
            //var appointment = state.Appointment as CustomAppointment;

            //if (appointment.IsReadOnly)
            //{
            //    return false;
            //}

            //return base.CanStartResize(state);

            return false;
        }

        public override void Resize(DragDropState state)
        {
            //var appointment = state.Appointment as CustomAppointment;
            //var destinationSlot = state.DestinationSlots.First() as Slot;
            //var duration = destinationSlot.End - destinationSlot.Start;
            //appointment.Body = "Resize finished. New duration: " + duration.ToString("h\\:mm\\:ss");
            base.Resize(state);
         }

        public override void ResizeCanceled(DragDropState state)
        {
            //var appointment = state.Appointment as CustomAppointment;
            //appointment.Body = "Resize Canceled";
            base.ResizeCanceled(state);
        }

        public override bool CanStartDrag(DragDropState state)
        {
            //var draggedAppointment = state.Appointment as CustomAppointment;

            //if (draggedAppointment.IsReadOnly)
            //{
            //    return false;
            //}

            //return base.CanStartDrag(state);
            return false;
        }

        public override void DragDropCanceled(DragDropState state)
        {
            //var appointment = state.Appointment as CustomAppointment;
            //appointment.Body = "DragDrop canceled at: " + DateTime.Now;
            base.DragDropCanceled(state);
    
        }

        public override void DragDropCompleted(DragDropState state)
        {
            //var appointment = state.Appointment as CustomAppointment;
            //appointment.Body = "DragDrop completed at: " + DateTime.Now;
            base.DragDropCompleted(state);
            
        }
    }
}
